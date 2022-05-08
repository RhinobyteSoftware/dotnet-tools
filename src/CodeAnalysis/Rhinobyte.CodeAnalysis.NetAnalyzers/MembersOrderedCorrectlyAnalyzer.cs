using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
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
	/// The diagnostic rule id for the analzyer
	/// </summary>
	public const string DiagnosticId = "RBCS0001";

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

	private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(
		DiagnosticId,
		DiagnosticDescriptorHelper.DesignCategory,
		new LocalizableResourceString(nameof(Resources.RBCS0001_AnalyzerDescription), Resources.ResourceManager, typeof(Resources)),
		DiagnosticSeverity.Info,
		new LocalizableResourceString(nameof(Resources.RBCS0001_AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources)),
		new LocalizableResourceString(nameof(Resources.RBCS0001_AnalyzerTitle), Resources.ResourceManager, typeof(Resources)),
		isEnabledByDefault: true
	);

	/// <inheritdoc />
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	private static void AnalyzeNamedTypeSymbol(SymbolAnalysisContext context)
	{
		var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;
		if (namedTypeSymbol.IsNamespace)
			return;

		if (namedTypeSymbol.Locations.Length > 1)
			return; // Partial class support not yet implemented

		var memberSymbols = namedTypeSymbol.GetMembers();

		// TODO: Define and load code style configuration values somehow
		var groupOrder = DefaultGroupOrder;

#if DEBUG
		var flattenedGroups = groupOrder.SelectMany(group => group.ToList()).ToArray();
		var allValues = System.Enum.GetValues(typeof(MemberGroupType)).Cast<MemberGroupType>().Where(value => value != MemberGroupType.Unknown).ToArray();
		Debug.Assert(flattenedGroups.Length == allValues.Length);
		Debug.Assert(flattenedGroups.All(value => allValues.Contains(value)));
		Debug.Assert(allValues.All(value => flattenedGroups.Contains(value)));
#endif

		var completedGroups = new List<MemberGroupType>();
		var currentGroup = groupOrder[0];
		var currentGroupIndex = 1;

		foreach (var memberSymbol in memberSymbols)
		{
			if (memberSymbol.IsImplicitlyDeclared || !memberSymbol.CanBeReferencedByName)
				continue;

			var currentMemberGroupType = GetGroupType(memberSymbol);
			if (currentMemberGroupType == MemberGroupType.InstanceMethods && memberSymbol.Name == namedTypeSymbol.Name)
				currentMemberGroupType = MemberGroupType.Constructors;

			if (completedGroups.Contains(currentMemberGroupType))
			{
				var diagnostic = Diagnostic.Create(Rule, memberSymbol.Locations[0], memberSymbol.Name);
				context.ReportDiagnostic(diagnostic);
				continue;
			}

			var isInCurrentGroup = currentGroup.Contains(currentMemberGroupType);
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
				var isConstructor = symbol.Name.Contains(".ctor");
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
}
