using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Rhinobyte.CodeAnalysis.NetAnalyzers.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Rhinobyte.CodeAnalysis.NetAnalyzers;

/// <summary>
/// Code analzyer for design time checks that type members are ordered alphabetically by member name for their respective groups
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class MembersOrderedCorrectlyAnalyzer : DiagnosticAnalyzer
{
	/// <summary>
	/// The diagnostic rule id for members being ordered correctly by group
	/// </summary>
	public const string RBCS0001 = "RBCS0001";

	/// <summary>
	/// The diagnostic rule id for members being ordered alphabetically within their respective groups
	/// </summary>
	public const string RBCS0002 = "RBCS0002";

	/// <summary>
	/// The default member group ordering for the analyzer
	/// </summary>
	internal static readonly MemberGroupType[][] DefaultGroupOrder = new[]
	{
		new MemberGroupType[] { MemberGroupType.NestedRecordType },
		new MemberGroupType[] { MemberGroupType.Constants, MemberGroupType.StaticReadonlyFields },
		new MemberGroupType[] { MemberGroupType.StaticMutableFields, MemberGroupType.StaticProperties },
		new MemberGroupType[] { MemberGroupType.StaticConstructors },
		new MemberGroupType[] { MemberGroupType.ReadonlyInstanceFields, MemberGroupType.MutableInstanceFields },
		new MemberGroupType[] { MemberGroupType.Constructors },
		new MemberGroupType[] { MemberGroupType.InstanceProperties },
		new MemberGroupType[] { MemberGroupType.InstanceMethods, MemberGroupType.StaticMethods },
		new MemberGroupType[] { MemberGroupType.NestedEnumType, MemberGroupType.NestedOtherType },
	};



	// You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
	// See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization

	internal static readonly DiagnosticDescriptor RuleRBCS0001 = DiagnosticDescriptorHelper.Create(
		RBCS0001,
		DiagnosticDescriptorHelper.DesignCategory,
		new LocalizableResourceString(nameof(Resources.RBCS0001_AnalyzerDescription), Resources.ResourceManager, typeof(Resources)),
		DiagnosticSeverity.Info,
		new LocalizableResourceString(nameof(Resources.RBCS0001_AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources)),
		new LocalizableResourceString(nameof(Resources.RBCS0001_AnalyzerTitle), Resources.ResourceManager, typeof(Resources)),
		isEnabledByDefault: true
	);

	internal static readonly DiagnosticDescriptor RuleRBCS0002 = DiagnosticDescriptorHelper.Create(
		RBCS0002,
		DiagnosticDescriptorHelper.DesignCategory,
		new LocalizableResourceString(nameof(Resources.RBCS0002_AnalyzerDescription), Resources.ResourceManager, typeof(Resources)),
		DiagnosticSeverity.Info,
		new LocalizableResourceString(nameof(Resources.RBCS0002_AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources)),
		new LocalizableResourceString(nameof(Resources.RBCS0002_AnalyzerTitle), Resources.ResourceManager, typeof(Resources)),
		isEnabledByDefault: true
	);

#pragma warning disable IDE0025 // Use expression body for properties
	/// <inheritdoc />
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(RuleRBCS0001, RuleRBCS0002); } }
#pragma warning restore IDE0025 // Use expression body for properties

	private static void AnalyzeMultipartNamedTypeSymbol(
		SymbolAnalysisContext context,
		MemberGroupType[][] groupOrder,
		INamedTypeSymbol namedTypeSymbol)
	{
		if (context.Symbol is null || groupOrder is null)
			return;

		Debug.Assert(namedTypeSymbol.DeclaringSyntaxReferences.Length == namedTypeSymbol.Locations.Length);
		if (namedTypeSymbol.DeclaringSyntaxReferences.Length != namedTypeSymbol.Locations.Length)
			return;

		var locationDataLookup = new NamedTypeLocationData[namedTypeSymbol.Locations.Length];
		for (var locationIndex = 0; locationIndex < namedTypeSymbol.Locations.Length; ++locationIndex)
		{
			var syntaxReference = namedTypeSymbol.DeclaringSyntaxReferences[locationIndex];
			var positionSpan = syntaxReference.SyntaxTree.GetLineSpan(syntaxReference.Span, context.CancellationToken);
			locationDataLookup[locationIndex] = new NamedTypeLocationData(groupOrder, namedTypeSymbol.Locations[locationIndex], positionSpan);
		}

		var memberSymbols = namedTypeSymbol.GetMembers();
		foreach (var memberSymbol in memberSymbols)
		{
			if (memberSymbol.IsImplicitlyDeclared)
				continue;

			var currentMemberGroupType = GetGroupType(memberSymbol);
			if (!memberSymbol.CanBeReferencedByName && currentMemberGroupType != MemberGroupType.Constructors && currentMemberGroupType != MemberGroupType.StaticConstructors)
				continue;

			foreach (var memberLocation in memberSymbol.Locations)
			{
				var memberLocationLineSpan = memberLocation.GetLineSpan();
				var locationMatches = 0;
				foreach (var locationData in locationDataLookup)
				{
					if (!SyntaxHelper.IsPositionSpanWithin(memberLocationLineSpan, locationData.PositionSpan))
						continue;

					++locationMatches;

					if (locationData.CompletedGroups.Contains(currentMemberGroupType))
					{
						var diagnostic = Diagnostic.Create(RuleRBCS0001, memberLocation, memberSymbol.Name);
						context.ReportDiagnostic(diagnostic);
						continue;
					}

					var isInCurrentGroup = locationData.CurrentGroup.Contains(currentMemberGroupType);
					if (isInCurrentGroup)
					{
						if (memberSymbol.CanBeReferencedByName
							&& locationData.PreviousMemberNameForCurrentGroup is not null
							&& string.Compare(locationData.PreviousMemberNameForCurrentGroup, memberSymbol.Name, StringComparison.OrdinalIgnoreCase) > 0)
						{
							var diagnostic = Diagnostic.Create(RuleRBCS0002, memberLocation, memberSymbol.Name);
							context.ReportDiagnostic(diagnostic);
							continue;
						}

						locationData.PreviousMemberNameForCurrentGroup = memberSymbol.Name;
						continue;
					}

					// If the symbol wasn't in the current group, update the previous member name to the first symbol for this new group
					locationData.PreviousMemberNameForCurrentGroup = memberSymbol.Name;

					isInCurrentGroup = locationData.AdvanceCurrentGroupTo(currentMemberGroupType, groupOrder);
					Debug.Assert(isInCurrentGroup);
				}

				Debug.Assert(locationMatches > 0);
			}
		}
	}

	private static void AnalyzeNamedTypeSymbol(SymbolAnalysisContext context)
	{
		var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;
		if (namedTypeSymbol.IsNamespace)
			return;

		var groupOrder = GetGroupOrderLookup(context);

#if DEBUG
		var flattenedGroups = groupOrder.SelectMany(group => group.ToList()).ToArray();
		var allValues = System.Enum.GetValues(typeof(MemberGroupType)).Cast<MemberGroupType>().Where(value => value != MemberGroupType.Unknown).ToArray();
		Debug.Assert(flattenedGroups.Length == allValues.Length);
		Debug.Assert(flattenedGroups.All(value => allValues.Contains(value)));
		Debug.Assert(allValues.All(value => flattenedGroups.Contains(value)));
#endif

		if (namedTypeSymbol.Locations.Length > 1)
		{
			AnalyzeMultipartNamedTypeSymbol(context, groupOrder, namedTypeSymbol);
			return;
		}

		var memberSymbols = namedTypeSymbol.GetMembers();
		var completedGroups = new List<MemberGroupType>();
		var currentGroup = groupOrder[0];
		var currentGroupIndex = 1;
		string? previousMemberNameForCurrentGroup = null;

		foreach (var memberSymbol in memberSymbols)
		{
			if (memberSymbol.IsImplicitlyDeclared)
				continue;

			var currentMemberGroupType = GetGroupType(memberSymbol);
			if (!memberSymbol.CanBeReferencedByName && currentMemberGroupType != MemberGroupType.Constructors && currentMemberGroupType != MemberGroupType.StaticConstructors)
				continue;

			if (completedGroups.Contains(currentMemberGroupType))
			{
				var diagnostic = Diagnostic.Create(RuleRBCS0001, memberSymbol.Locations[0], memberSymbol.Name);
				context.ReportDiagnostic(diagnostic);
				continue;
			}

			var isInCurrentGroup = currentGroup.Contains(currentMemberGroupType);
			if (isInCurrentGroup)
			{
				if (memberSymbol.CanBeReferencedByName
					&& previousMemberNameForCurrentGroup is not null
					&& string.Compare(previousMemberNameForCurrentGroup, memberSymbol.Name, StringComparison.OrdinalIgnoreCase) > 0)
				{
					var diagnostic = Diagnostic.Create(RuleRBCS0002, memberSymbol.Locations[0], memberSymbol.Name);
					context.ReportDiagnostic(diagnostic);
					continue;
				}

				previousMemberNameForCurrentGroup = memberSymbol.Name;
				continue;
			}

			// If the symbol wasn't in the current group, update the previous member name to the first symbol for this new group
			previousMemberNameForCurrentGroup = memberSymbol.Name;

			while (!isInCurrentGroup && currentGroupIndex < groupOrder.Length)
			{
				completedGroups.AddRange(currentGroup);
				currentGroup = groupOrder[currentGroupIndex++];
				isInCurrentGroup = currentGroup.Contains(currentMemberGroupType);
			}

			Debug.Assert(isInCurrentGroup);
		}
	}

	/// <inheritdoc />
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Unnecessary in code analyzer extension")]
	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();

		// TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
		// See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
		context.RegisterSymbolAction(AnalyzeNamedTypeSymbol, SymbolKind.NamedType);
	}

	internal static MemberGroupType[][] GetGroupOrderLookup(SymbolAnalysisContext context)
	{
		try
		{
			var groupOrderStringValue = context.Options.GetStringOptionValue("type_members_group_order", context.Compilation, RuleRBCS0001, null, string.Empty);
			if (!string.IsNullOrEmpty(groupOrderStringValue))
			{
				// TODO: Parse option value string and generate the group order lookup from it if possible...
			}
		}
#pragma warning disable CA1031 // Do not catch general exception types
#pragma warning disable CS0168 // Variable is declared but never used
		catch (Exception parseOptionsException)
#pragma warning restore CS0168 // Variable is declared but never used
#pragma warning restore CA1031 // Do not catch general exception types
		{
#if DEBUG
			System.Diagnostics.Debugger.Break();
#endif
		}

		return DefaultGroupOrder;

	}

	internal static MemberGroupType GetGroupType(ISymbol symbol)
	{
		switch (symbol.Kind)
		{
			case SymbolKind.Field:
			{
				var fieldSymbol = (IFieldSymbol)symbol;
				if (fieldSymbol.IsConst)
					return MemberGroupType.Constants;
				else if (fieldSymbol.IsStatic && fieldSymbol.IsReadOnly)
					return MemberGroupType.StaticReadonlyFields;
				else if (fieldSymbol.IsStatic)
					return MemberGroupType.StaticMutableFields;
				else if (fieldSymbol.IsReadOnly)
					return MemberGroupType.MutableInstanceFields;
				else
					return MemberGroupType.ReadonlyInstanceFields;

			}

			case SymbolKind.Method:
			{
				var isConstructor = symbol.Name.Contains(".ctor") || symbol.Name.Contains(".cctor");
				if (isConstructor && symbol.IsStatic)
					return MemberGroupType.StaticConstructors;
				else if (isConstructor)
					return MemberGroupType.Constructors;
				else if (symbol.IsStatic)
					return MemberGroupType.StaticMethods;
				else
					return MemberGroupType.InstanceMethods;
			}

			case SymbolKind.Property:
			{
				var propertySymbol = (IPropertySymbol)symbol;
				if (propertySymbol.IsStatic)
					return MemberGroupType.StaticProperties;
				else
					return MemberGroupType.InstanceProperties;
			}

			case SymbolKind.NamedType:
			{
				var nestedTypeSymbol = (INamedTypeSymbol)symbol;
				if (nestedTypeSymbol.IsRecord)
					return MemberGroupType.NestedRecordType;

				if (nestedTypeSymbol.TypeKind == TypeKind.Enum)
					return MemberGroupType.NestedEnumType;

				if (nestedTypeSymbol.IsType)
					return MemberGroupType.NestedOtherType;

				return MemberGroupType.Unknown;
			}

			case SymbolKind.Alias:
			case SymbolKind.ArrayType:
			case SymbolKind.Assembly:
			case SymbolKind.Discard:
			case SymbolKind.DynamicType:
			case SymbolKind.ErrorType:
			case SymbolKind.Event:
			case SymbolKind.FunctionPointerType:
			case SymbolKind.Label:
			case SymbolKind.Local:
			case SymbolKind.Namespace:
			case SymbolKind.NetModule:
			case SymbolKind.Parameter:
			case SymbolKind.PointerType:
			case SymbolKind.Preprocessing:
			case SymbolKind.RangeVariable:
			case SymbolKind.TypeParameter:
			default:
				return MemberGroupType.Unknown;
		}
	}

	internal class NamedTypeLocationData
	{
		public NamedTypeLocationData(
			MemberGroupType[][] groupOrder,
			Location location,
			FileLinePositionSpan positionSpan)
		{
			CompletedGroups = new List<MemberGroupType>();
			CurrentGroup = groupOrder[0];
			CurrentGroupIndex = 1;
			Location = location;
			PositionSpan = positionSpan;
			PreviousMemberNameForCurrentGroup = null;
		}

		internal List<MemberGroupType> CompletedGroups { get; }
		internal MemberGroupType[] CurrentGroup { get; private set; }
		internal int CurrentGroupIndex { get; private set; }
		internal Location Location { get; }
		internal FileLinePositionSpan PositionSpan { get; }
		internal string? PreviousMemberNameForCurrentGroup { get; set; }

		internal bool AdvanceCurrentGroupTo(MemberGroupType groupForCurrentMemberSymbol, MemberGroupType[][] groupOrder)
		{
			var isInCurrentGroup = CurrentGroup.Contains(groupForCurrentMemberSymbol);
			while (!isInCurrentGroup && CurrentGroupIndex < groupOrder.Length)
			{
				CompletedGroups.AddRange(CurrentGroup);
				CurrentGroup = groupOrder[CurrentGroupIndex++];
				isInCurrentGroup = CurrentGroup.Contains(groupForCurrentMemberSymbol);
			}

			return isInCurrentGroup;
		}
	}
}
