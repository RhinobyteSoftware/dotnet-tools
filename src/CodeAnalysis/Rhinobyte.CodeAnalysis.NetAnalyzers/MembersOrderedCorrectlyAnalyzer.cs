﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
	public const string RBCS0001 = nameof(RBCS0001);

	/// <summary>
	/// The diagnostic rule id for members being ordered alphabetically within their respective groups
	/// </summary>
	public const string RBCS0002 = nameof(RBCS0002);

	/// <summary>
	/// The diagnostic rule id for member names in an object initializer being ordered alphabetically
	/// </summary>
	public const string RBCS0003 = nameof(RBCS0003);

	/// <summary>
	/// The diagnostic rule id for enum member names not being ordered alphabetically
	/// </summary>
	public const string RBCS0004 = nameof(RBCS0004);

	/// <summary>
	/// The diagnostic rule id for method parameter names not being ordered alphabetically
	/// </summary>
	public const string RBCS0005 = nameof(RBCS0005);

	/// <summary>
	/// The diagnostic rule id for record type member names not being ordered alphabetically
	/// </summary>
	public const string RBCS0006 = nameof(RBCS0006);

	// You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
	// See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization

	internal static readonly DiagnosticDescriptor Rule_RBCS_0001 = DiagnosticDescriptorHelper.Create(
		RBCS0001,
		DiagnosticDescriptorHelper.MaintainabilityCategory,
		new LocalizableResourceString(nameof(AnalyzerResources.RBCS_0001_AnalyzerDescription), AnalyzerResources.ResourceManager, typeof(AnalyzerResources)),
		DiagnosticSeverity.Info,
		new LocalizableResourceString(nameof(AnalyzerResources.RBCS_0001_AnalyzerMessageFormat), AnalyzerResources.ResourceManager, typeof(AnalyzerResources)),
		new LocalizableResourceString(nameof(AnalyzerResources.RBCS_0001_AnalyzerTitle), AnalyzerResources.ResourceManager, typeof(AnalyzerResources)),
		isEnabledByDefault: true
	);

	internal static readonly DiagnosticDescriptor Rule_RBCS_0002 = DiagnosticDescriptorHelper.Create(
		RBCS0002,
		DiagnosticDescriptorHelper.MaintainabilityCategory,
		new LocalizableResourceString(nameof(AnalyzerResources.RBCS_0002_AnalyzerDescription), AnalyzerResources.ResourceManager, typeof(AnalyzerResources)),
		DiagnosticSeverity.Info,
		new LocalizableResourceString(nameof(AnalyzerResources.RBCS_0002_AnalyzerMessageFormat), AnalyzerResources.ResourceManager, typeof(AnalyzerResources)),
		new LocalizableResourceString(nameof(AnalyzerResources.RBCS_0002_AnalyzerTitle), AnalyzerResources.ResourceManager, typeof(AnalyzerResources)),
		isEnabledByDefault: true
	);

	internal static readonly DiagnosticDescriptor Rule_RBCS_0003 = DiagnosticDescriptorHelper.Create(
		RBCS0003,
		DiagnosticDescriptorHelper.MaintainabilityCategory,
		new LocalizableResourceString(nameof(AnalyzerResources.RBCS_0003_AnalyzerDescription), AnalyzerResources.ResourceManager, typeof(AnalyzerResources)),
		DiagnosticSeverity.Info,
		new LocalizableResourceString(nameof(AnalyzerResources.RBCS_0003_AnalyzerMessageFormat), AnalyzerResources.ResourceManager, typeof(AnalyzerResources)),
		new LocalizableResourceString(nameof(AnalyzerResources.RBCS_0003_AnalyzerTitle), AnalyzerResources.ResourceManager, typeof(AnalyzerResources)),
		isEnabledByDefault: true
	);

	internal static readonly DiagnosticDescriptor Rule_RBCS_0004 = DiagnosticDescriptorHelper.Create(
		RBCS0004,
		DiagnosticDescriptorHelper.MaintainabilityCategory,
		new LocalizableResourceString(nameof(AnalyzerResources.RBCS_0004_AnalyzerDescription), AnalyzerResources.ResourceManager, typeof(AnalyzerResources)),
		DiagnosticSeverity.Hidden,
		new LocalizableResourceString(nameof(AnalyzerResources.RBCS_0004_AnalyzerMessageFormat), AnalyzerResources.ResourceManager, typeof(AnalyzerResources)),
		new LocalizableResourceString(nameof(AnalyzerResources.RBCS_0004_AnalyzerTitle), AnalyzerResources.ResourceManager, typeof(AnalyzerResources)),
		isEnabledByDefault: false
	);

	internal static readonly DiagnosticDescriptor Rule_RBCS_0005 = DiagnosticDescriptorHelper.Create(
		RBCS0005,
		DiagnosticDescriptorHelper.MaintainabilityCategory,
		new LocalizableResourceString(nameof(AnalyzerResources.RBCS_0005_AnalyzerDescription), AnalyzerResources.ResourceManager, typeof(AnalyzerResources)),
		DiagnosticSeverity.Hidden,
		new LocalizableResourceString(nameof(AnalyzerResources.RBCS_0005_AnalyzerMessageFormat), AnalyzerResources.ResourceManager, typeof(AnalyzerResources)),
		new LocalizableResourceString(nameof(AnalyzerResources.RBCS_0005_AnalyzerTitle), AnalyzerResources.ResourceManager, typeof(AnalyzerResources)),
		isEnabledByDefault: false
	);

	internal static readonly DiagnosticDescriptor Rule_RBCS_0006 = DiagnosticDescriptorHelper.Create(
		RBCS0006,
		DiagnosticDescriptorHelper.MaintainabilityCategory,
		new LocalizableResourceString(nameof(AnalyzerResources.RBCS_0006_AnalyzerDescription), AnalyzerResources.ResourceManager, typeof(AnalyzerResources)),
		DiagnosticSeverity.Hidden,
		new LocalizableResourceString(nameof(AnalyzerResources.RBCS_0006_AnalyzerMessageFormat), AnalyzerResources.ResourceManager, typeof(AnalyzerResources)),
		new LocalizableResourceString(nameof(AnalyzerResources.RBCS_0006_AnalyzerTitle), AnalyzerResources.ResourceManager, typeof(AnalyzerResources)),
		isEnabledByDefault: false
	);

	/// <inheritdoc />
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
	[
		Rule_RBCS_0001,
		Rule_RBCS_0002,
		Rule_RBCS_0003,
		Rule_RBCS_0004,
		Rule_RBCS_0005,
		Rule_RBCS_0006
	];

	private static void AnalyzeMethodDeclarationSyntax(SyntaxNodeAnalysisContext context)
	{
		var classDeclarationSyntax = context.Node as ClassDeclarationSyntax;
		var constructorDeclarationSyntax = context.Node as ConstructorDeclarationSyntax;
		var methodDeclarationSyntax = context.Node as MethodDeclarationSyntax;

		if (classDeclarationSyntax is null
			&& constructorDeclarationSyntax is null
			&& methodDeclarationSyntax is null)
		{
			return;
		}

		var parameterList = classDeclarationSyntax?.ParameterList
			?? constructorDeclarationSyntax?.ParameterList
			?? methodDeclarationSyntax?.ParameterList;

		if (parameterList is null)
			return;

		var parameterCount = parameterList.Parameters.Count;
		if (parameterCount < 1)
			return;

		var outOfOrderParameterNames = new List<string>();
		string? previousParameterName = null;

		foreach (var parameterSyntax in parameterList.Parameters)
		{
			var parameterName = parameterSyntax.Identifier.ValueText ?? parameterSyntax.Identifier.Text;
			if (string.IsNullOrEmpty(parameterName))
				continue;

			if (previousParameterName is not null
				&& string.Compare(previousParameterName, parameterName, StringComparison.OrdinalIgnoreCase) > 0)
			{
				outOfOrderParameterNames.Add(parameterName);
				continue;
			}

			previousParameterName = parameterName;
		}

		if (outOfOrderParameterNames.Count > 0)
		{
			var diagnostic = Diagnostic.Create(Rule_RBCS_0005, parameterList.GetLocation(), string.Join(", ", outOfOrderParameterNames));
			context.ReportDiagnostic(diagnostic);
		}
	}

	private static void AnalyzeMultipartNamedTypeSymbol(
		in SymbolAnalysisContext context,
		INamedTypeSymbol namedTypeSymbol)
	{
		Debug.Assert(namedTypeSymbol.DeclaringSyntaxReferences.Length == namedTypeSymbol.Locations.Length);
		if (namedTypeSymbol.DeclaringSyntaxReferences.Length != namedTypeSymbol.Locations.Length)
			return;

		var orderingOptions = MemberOrderingOptions.ParseOptions(context);
		var groupOrder = MemberOrderingOptions.GetGroupOrderLookupOrDefault(orderingOptions.GroupOrderSettings);

		var methodNamesToOrderFirst = MemberOrderingOptions.GetMemberNamesToOrderFirst(orderingOptions.MethodNamesToOrderFirst);
		var propertyNamesToOrderFirst = MemberOrderingOptions.GetMemberNamesToOrderFirst(orderingOptions.PropertyNamesToOrderFirst);

		var propertyNameOrderComparer = orderingOptions.ArePropertyNamesToOrderFirstCaseSensitive
			? StringComparer.Ordinal
			: StringComparer.OrdinalIgnoreCase;

		ImmutableDictionary<string, string?>? diagnosticProperties = null;

		var locationDataLookup = new NamedTypeLocationData[namedTypeSymbol.Locations.Length];
		for (var locationIndex = 0; locationIndex < namedTypeSymbol.Locations.Length; ++locationIndex)
		{
			var syntaxReference = namedTypeSymbol.DeclaringSyntaxReferences[locationIndex];
			var positionSpan = syntaxReference.SyntaxTree.GetLineSpan(syntaxReference.Span, context.CancellationToken);
			locationDataLookup[locationIndex] = new NamedTypeLocationData(groupOrder, namedTypeSymbol.Locations[locationIndex], positionSpan);
		}

		var memberSymbols = namedTypeSymbol.GetMembers();
		var alphabeticalMemberRuleDescriptorForType = SelectAlphabeticalMemberRuleDescriptorToUse(namedTypeSymbol);

		foreach (var memberSymbol in memberSymbols)
		{
			if (memberSymbol.IsImplicitlyDeclared)
				continue;

			var currentMemberGroupType = GetGroupType(memberSymbol);
			if (!memberSymbol.CanBeReferencedByName && currentMemberGroupType != MemberGroupType.Constructors && currentMemberGroupType != MemberGroupType.StaticConstructors)
				continue;

			if (currentMemberGroupType == MemberGroupType.Constructors)
			{
				var declaringSyntax = memberSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(context.CancellationToken);
				var isPrimaryConstructor = declaringSyntax is ClassDeclarationSyntax;

				if (isPrimaryConstructor)
					continue;
			}

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
						diagnosticProperties ??= MemberOrderingOptions.BuildDiagnosticPropertiesDictionary(orderingOptions);
						var diagnostic = Diagnostic.Create(Rule_RBCS_0001, memberLocation, properties: diagnosticProperties, memberSymbol.Name);
						context.ReportDiagnostic(diagnostic);
						continue;
					}

					var isMethodType = currentMemberGroupType == MemberGroupType.InstanceMethods || currentMemberGroupType == MemberGroupType.StaticMethods;
					var isInstancePropertyType = currentMemberGroupType == MemberGroupType.InstanceProperties;

					var isInCurrentGroup = locationData.CurrentGroup.Contains(currentMemberGroupType);
					if (isInCurrentGroup)
					{
						if (!memberSymbol.CanBeReferencedByName)
							continue;

						var shouldBeOrderedFirst = false;
						if (isMethodType)
						{
							shouldBeOrderedFirst = methodNamesToOrderFirst is not null
								&& methodNamesToOrderFirst.Contains(memberSymbol.Name, StringComparer.OrdinalIgnoreCase);
						}
						else if (isInstancePropertyType)
						{
							shouldBeOrderedFirst = propertyNamesToOrderFirst is not null
								&& propertyNamesToOrderFirst.Contains(memberSymbol.Name, propertyNameOrderComparer);
						}

						if (shouldBeOrderedFirst && !locationData.IsAlphabetizingGroup)
							continue;

						if (shouldBeOrderedFirst
							|| (locationData.PreviousMemberNameForCurrentGroup is not null
								&& locationData.IsAlphabetizingGroup
								&& string.Compare(locationData.PreviousMemberNameForCurrentGroup, memberSymbol.Name, StringComparison.OrdinalIgnoreCase) > 0))
						{
							diagnosticProperties ??= MemberOrderingOptions.BuildDiagnosticPropertiesDictionary(orderingOptions);

							var diagnostic = Diagnostic.Create(alphabeticalMemberRuleDescriptorForType, memberLocation, properties: diagnosticProperties, memberSymbol.Name);
							context.ReportDiagnostic(diagnostic);
							continue;
						}

						locationData.IsAlphabetizingGroup = true;
						locationData.PreviousMemberNameForCurrentGroup = memberSymbol.Name;
						continue;
					}

					// If the symbol wasn't in the current group, update the previous member name to the first symbol for this new group


					// If the symbol wasn't in the current group, update the previous member name to the first symbol for this new group
					if (isMethodType)
					{
						locationData.IsAlphabetizingGroup = methodNamesToOrderFirst is null || !methodNamesToOrderFirst.Contains(memberSymbol.Name, StringComparer.OrdinalIgnoreCase);
					}
					else if (isInstancePropertyType)
					{
						locationData.IsAlphabetizingGroup = propertyNamesToOrderFirst is null || !propertyNamesToOrderFirst.Contains(memberSymbol.Name, propertyNameOrderComparer);
					}
					else
					{
						locationData.IsAlphabetizingGroup = true;
					}

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

		if (namedTypeSymbol.Locations.Length > 1)
		{
			AnalyzeMultipartNamedTypeSymbol(context, namedTypeSymbol);
			return;
		}

		var orderingOptions = MemberOrderingOptions.ParseOptions(context);
		var groupOrder = MemberOrderingOptions.GetGroupOrderLookupOrDefault(orderingOptions.GroupOrderSettings);

		var methodNamesToOrderFirst = MemberOrderingOptions.GetMemberNamesToOrderFirst(orderingOptions.MethodNamesToOrderFirst);

		var propertyNamesToOrderFirst = MemberOrderingOptions.GetMemberNamesToOrderFirst(orderingOptions.PropertyNamesToOrderFirst);
		var propertyNameOrderComparer = orderingOptions.ArePropertyNamesToOrderFirstCaseSensitive
			? StringComparer.Ordinal
			: StringComparer.OrdinalIgnoreCase;

		ImmutableDictionary<string, string?>? diagnosticProperties = null;

		var memberSymbols = namedTypeSymbol.GetMembers();

		var alphabeticalMemberRuleDescriptorForType = SelectAlphabeticalMemberRuleDescriptorToUse(namedTypeSymbol);

		var completedGroups = new List<MemberGroupType>();
		var currentGroup = groupOrder[0];
		var currentGroupIndex = 1;
		string? previousMemberNameForCurrentGroup = null;
		var isAlphabetizingGroup = false;

		foreach (var memberSymbol in memberSymbols)
		{
			if (memberSymbol.IsImplicitlyDeclared)
				continue;

			var currentMemberGroupType = GetGroupType(memberSymbol);
			if (!memberSymbol.CanBeReferencedByName && currentMemberGroupType != MemberGroupType.Constructors && currentMemberGroupType != MemberGroupType.StaticConstructors)
				continue;

			if (currentMemberGroupType == MemberGroupType.Constructors)
			{
				var declaringSyntax = memberSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(context.CancellationToken);
				var isPrimaryConstructor = declaringSyntax is ClassDeclarationSyntax;

				if (isPrimaryConstructor)
					continue;
			}

			if (completedGroups.Contains(currentMemberGroupType))
			{
				diagnosticProperties ??= MemberOrderingOptions.BuildDiagnosticPropertiesDictionary(orderingOptions);
				var diagnostic = Diagnostic.Create(Rule_RBCS_0001, memberSymbol.Locations[0], properties: diagnosticProperties, memberSymbol.Name);
				context.ReportDiagnostic(diagnostic);
				continue;
			}

			var isMethodType = currentMemberGroupType == MemberGroupType.InstanceMethods || currentMemberGroupType == MemberGroupType.StaticMethods;
			var isInstancePropertyType = currentMemberGroupType == MemberGroupType.InstanceProperties;

			var isInCurrentGroup = currentGroup.Contains(currentMemberGroupType);
			if (isInCurrentGroup)
			{
				if (!memberSymbol.CanBeReferencedByName)
					continue;

				var shouldBeOrderedFirst = false;
				if (isMethodType)
				{
					shouldBeOrderedFirst = methodNamesToOrderFirst is not null
						&& methodNamesToOrderFirst.Contains(memberSymbol.Name, StringComparer.OrdinalIgnoreCase);
				}
				else if (isInstancePropertyType)
				{
					shouldBeOrderedFirst = propertyNamesToOrderFirst is not null
						&& propertyNamesToOrderFirst.Contains(memberSymbol.Name, propertyNameOrderComparer);
				}

				if (shouldBeOrderedFirst && !isAlphabetizingGroup)
					continue;

				if (shouldBeOrderedFirst
					|| (previousMemberNameForCurrentGroup is not null
						&& isAlphabetizingGroup
						&& string.Compare(previousMemberNameForCurrentGroup, memberSymbol.Name, StringComparison.OrdinalIgnoreCase) > 0))
				{
					diagnosticProperties ??= MemberOrderingOptions.BuildDiagnosticPropertiesDictionary(orderingOptions);

					var diagnostic = Diagnostic.Create(alphabeticalMemberRuleDescriptorForType, memberSymbol.Locations[0], properties: diagnosticProperties, memberSymbol.Name);
					context.ReportDiagnostic(diagnostic);
					continue;
				}

				isAlphabetizingGroup = true;
				previousMemberNameForCurrentGroup = memberSymbol.Name;
				continue;
			}

			// If the symbol wasn't in the current group, update the previous member name to the first symbol for this new group
			if (isMethodType)
			{
				isAlphabetizingGroup = methodNamesToOrderFirst is null || !methodNamesToOrderFirst.Contains(memberSymbol.Name, StringComparer.OrdinalIgnoreCase);
			}
			else if (isInstancePropertyType)
			{
				isAlphabetizingGroup = propertyNamesToOrderFirst is null || !propertyNamesToOrderFirst.Contains(memberSymbol.Name, propertyNameOrderComparer);
			}
			else
			{
				isAlphabetizingGroup = true;
			}

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

	private static void AnalyzeObjectInitializerSyntax(SyntaxNodeAnalysisContext context)
	{
		if (context.Node is not InitializerExpressionSyntax initializerExpressionSyntax)
			return;

		var orderingOptions = MemberOrderingOptions.ParseOptions(context);
		var propertyNamesToOrderFirst = MemberOrderingOptions.GetMemberNamesToOrderFirst(orderingOptions.PropertyNamesToOrderFirst);
		var propertyNameOrderComparer = orderingOptions.ArePropertyNamesToOrderFirstCaseSensitive
			? StringComparer.Ordinal
			: StringComparer.OrdinalIgnoreCase;

		var isAlphabetizing = propertyNamesToOrderFirst is null;
		string? previousIdentifierName = null;

		var childNodes = initializerExpressionSyntax.ChildNodes();
		var outOfOrderMemberAssignments = new List<string>();
		foreach (var childNode in childNodes)
		{
			if (childNode is not AssignmentExpressionSyntax assignmentExpressionSyntax)
				continue;

			var leftNode = assignmentExpressionSyntax.Left;
			if (leftNode is not IdentifierNameSyntax identifierNameSyntax)
				continue;

			var identifierName = identifierNameSyntax.Identifier.ValueText ?? identifierNameSyntax.Identifier.Text;
			if (string.IsNullOrEmpty(identifierName))
				continue;

			var shouldBeOrderedFirst = propertyNamesToOrderFirst is not null
				&& propertyNamesToOrderFirst.Contains(identifierName, propertyNameOrderComparer);

			if (shouldBeOrderedFirst && !isAlphabetizing)
				continue;

			if (shouldBeOrderedFirst
				|| (previousIdentifierName is not null
					&& isAlphabetizing
					&& string.Compare(previousIdentifierName, identifierName, StringComparison.OrdinalIgnoreCase) > 0))
			{
				outOfOrderMemberAssignments.Add(identifierName);
				continue;
			}

			isAlphabetizing = true;
			previousIdentifierName = identifierName;
		}

		if (outOfOrderMemberAssignments.Count > 0)
		{
			var diagnosticProperties = MemberOrderingOptions.BuildDiagnosticPropertiesDictionary(orderingOptions);
			var diagnostic = Diagnostic.Create(Rule_RBCS_0003, initializerExpressionSyntax.GetLocation(), properties: diagnosticProperties, string.Join(", ", outOfOrderMemberAssignments));
			context.ReportDiagnostic(diagnostic);
		}
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
				var methodSymbol = symbol as IMethodSymbol;
				var isConstructor = methodSymbol?.MethodKind == MethodKind.Constructor
					|| symbol.Name.Contains(".ctor")
					|| symbol.Name.Contains(".cctor");

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

	/// <inheritdoc />
	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();

		// TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
		// See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
		context.RegisterSymbolAction(AnalyzeNamedTypeSymbol, SymbolKind.NamedType);

		context.RegisterSyntaxNodeAction<Microsoft.CodeAnalysis.CSharp.SyntaxKind>(
			AnalyzeObjectInitializerSyntax,
			Microsoft.CodeAnalysis.CSharp.SyntaxKind.ObjectInitializerExpression
		);

		context.RegisterSyntaxNodeAction<Microsoft.CodeAnalysis.CSharp.SyntaxKind>(
			AnalyzeMethodDeclarationSyntax,
			Microsoft.CodeAnalysis.CSharp.SyntaxKind.ClassDeclaration,
			Microsoft.CodeAnalysis.CSharp.SyntaxKind.ConstructorDeclaration,
			Microsoft.CodeAnalysis.CSharp.SyntaxKind.MethodDeclaration
		);
	}

	/// <summary>
	/// Select the specific diagnostic rule descriptor for alphabetizing members to use based on the provided named type symbol
	/// </summary>
	/// <remarks>
	/// Returns Rule_RBCS_0004 for enum types, Rule_RBCS_0006 for record types, and RuleRBCS0002 for all other named types
	/// </remarks>
	public static DiagnosticDescriptor SelectAlphabeticalMemberRuleDescriptorToUse(INamedTypeSymbol namedTypeSymbol)
	{
		if (namedTypeSymbol.TypeKind == TypeKind.Enum)
			return Rule_RBCS_0004;

		if (namedTypeSymbol.IsRecord)
			return Rule_RBCS_0006;

		return Rule_RBCS_0002;
	}

	internal class NamedTypeLocationData(
		MemberGroupType[][] groupOrder,
		Location location,
		FileLinePositionSpan positionSpan)
	{
		internal List<MemberGroupType> CompletedGroups { get; } = [];
		internal MemberGroupType[] CurrentGroup { get; private set; } = groupOrder[0];
		internal int CurrentGroupIndex { get; private set; } = 1;
		internal Location Location { get; } = location;
		internal bool IsAlphabetizingGroup { get; set; }
		internal FileLinePositionSpan PositionSpan { get; } = positionSpan;
		internal string? PreviousMemberNameForCurrentGroup { get; set; }

		internal bool AdvanceCurrentGroupTo(in MemberGroupType groupForCurrentMemberSymbol, MemberGroupType[][] groupOrder)
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
