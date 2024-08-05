using Analyzer.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Rhinobyte.CodeAnalysis.NetAnalyzers.Utilities;

internal readonly struct MemberOrderingOptions
{
	/// <summary>
	/// The default member group ordering for the analyzer
	/// </summary>
	internal static readonly MemberGroupType[][] DefaultGroupOrder =
	[
		[MemberGroupType.NestedRecordType],
		[MemberGroupType.Constants, MemberGroupType.StaticReadonlyFields],
		[MemberGroupType.StaticMutableFields, MemberGroupType.StaticProperties],
		[MemberGroupType.StaticConstructors],
		[MemberGroupType.ReadonlyInstanceFields, MemberGroupType.MutableInstanceFields],
		[MemberGroupType.Constructors],
		[MemberGroupType.InstanceProperties],
		[MemberGroupType.InstanceMethods, MemberGroupType.StaticMethods],
		[MemberGroupType.NestedEnumType, MemberGroupType.NestedOtherType],
	];

	private MemberOrderingOptions(
		bool arePropertyNamesToOrderFirstCaseSensitive,
		string? groupOrderSettings,
		string? propertyNamesToOrderFirst)
	{
		ArePropertyNamesToOrderFirstCaseSensitive = arePropertyNamesToOrderFirstCaseSensitive;
		GroupOrderSettings = groupOrderSettings;
		PropertyNamesToOrderFirst = propertyNamesToOrderFirst;
	}

	/// <summary>
	/// Whether or not the <see cref="PropertyNamesToOrderFirst"/> comparison should be case sensitive.
	/// False (case-insensitive) by default.
	/// <para>
	/// Can be specified from the .EditorConfig via:
	/// <code>dotnet_code_quality.RBCS0002.property_names_to_order_first_are_case_sensitive</code>
	/// </para>
	/// </summary>
	public bool ArePropertyNamesToOrderFirstCaseSensitive { get; }

	/// <summary>
	/// The raw value for the group order.
	/// <para>
	/// Can be specified from the .EditorConfig via:
	/// <code>dotnet_code_quality.RBCS0001.type_members_group_order</code>
	/// </para>
	/// </summary>
	public string? GroupOrderSettings { get; }

	/// <summary>
	/// The optional set of property names to order first before alphabetizing.
	/// <para>
	/// Can be specified from the .EditorConfig via:
	/// <code>dotnet_code_quality.RBCS0002.property_names_to_order_first</code>
	/// </para>
	/// </summary>
	public string? PropertyNamesToOrderFirst { get; }


	internal static ImmutableDictionary<string, string?> BuildDiagnosticPropertiesDictionary(in MemberOrderingOptions memberOrderingOptions)
	{
		return new Dictionary<string, string?>()
		{
			{ nameof(ArePropertyNamesToOrderFirstCaseSensitive), memberOrderingOptions.ArePropertyNamesToOrderFirstCaseSensitive.ToString() },
			{ nameof(GroupOrderSettings), memberOrderingOptions.GroupOrderSettings },
			{ nameof(PropertyNamesToOrderFirst), memberOrderingOptions.PropertyNamesToOrderFirst },
		}
		.ToImmutableDictionary();
	}

	internal static string ConvertGroupOrderLookupToString(MemberGroupType[][] groupOrder)
		=> string.Join(":", groupOrder.Select(grouping => string.Join(",", grouping)));

	internal static MemberGroupType[][]? ConvertStringToGroupOrderLookup(string? groupOrderLookupString)
	{
		if (string.IsNullOrEmpty(groupOrderLookupString))
			return null;

		var groupOrderLookup = groupOrderLookupString!
			.Split(':')
			.Select(grouping =>
				grouping
					.Split(',')
					.Select(memberGroupTypeStringValue => Enum.TryParse<MemberGroupType>(memberGroupTypeStringValue.Trim(), out var parsedValue) ? parsedValue : MemberGroupType.Unknown)
					.ToArray()
			)
			.ToArray();

		return groupOrderLookup;
	}

	internal static MemberGroupType[][] GetGroupOrderLookupOrDefault(string? groupOrderSettings)
	{
		try
		{
			var groupOrderLookup = ConvertStringToGroupOrderLookup(groupOrderSettings);
			if (groupOrderLookup is null)
				return DefaultGroupOrder;

			var flattenedGroups = groupOrderLookup.SelectMany(group => group.ToList()).ToArray();
			var allValues = Enum.GetValues(typeof(MemberGroupType)).Cast<MemberGroupType>().Where(value => value != MemberGroupType.Unknown).ToArray();

			if (flattenedGroups.Length != allValues.Length
				|| flattenedGroups.Any(value => !allValues.Contains(value))
				|| allValues.Any(value => !flattenedGroups.Contains(value)))
			{
				// If the parsed values aren't valid fallback to the defaults
				return DefaultGroupOrder;
			}

			return groupOrderLookup;
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

	internal static string[]? GetPropertyNamesToOrderFirst(string? propertyNamesToOrderFirstRawValue)
	{
		if (string.IsNullOrEmpty(propertyNamesToOrderFirstRawValue))
			return null;

		return propertyNamesToOrderFirstRawValue!
			.Split(',')
			.Select(propertyName => propertyName.Trim())
			.Where(propertyName => !string.IsNullOrEmpty(propertyName))
			.ToArray();
	}

	internal static MemberOrderingOptions ParseOptions(in SymbolAnalysisContext context)
		=> ParseOptions(context.Options, context.Compilation);

	internal static MemberOrderingOptions ParseOptions(in SyntaxNodeAnalysisContext context)
		=> ParseOptions(context.Options, context.Compilation);

	internal static MemberOrderingOptions ParseOptions(AnalyzerOptions analyzerOptions, Compilation compilation)
	{
		if (analyzerOptions is null || compilation is null)
			return new MemberOrderingOptions();

		try
		{
			var syntaxTree = compilation.SyntaxTrees.FirstOrDefault();
			var groupOrderSettings = analyzerOptions.GetStringOptionValue("type_members_group_order", MembersOrderedCorrectlyAnalyzer.RuleRBCS0001, syntaxTree, compilation);
			var propertyNamesToOrderFirst = analyzerOptions.GetStringOptionValue("property_names_to_order_first", MembersOrderedCorrectlyAnalyzer.RuleRBCS0002, syntaxTree, compilation);
			var arePropertyNamesToOrderFirstCaseSensitive = analyzerOptions
				.GetBoolOptionValue("property_names_to_order_first_are_case_sensitive", MembersOrderedCorrectlyAnalyzer.RuleRBCS0002, syntaxTree, compilation, false);

			return new MemberOrderingOptions(
				arePropertyNamesToOrderFirstCaseSensitive,
				groupOrderSettings,
				propertyNamesToOrderFirst
			);
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

		return new MemberOrderingOptions();
	}

}
