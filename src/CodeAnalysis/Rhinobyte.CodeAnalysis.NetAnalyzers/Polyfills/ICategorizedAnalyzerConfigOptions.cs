﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See https://github.com/dotnet/roslyn-analyzers/blob/release/6.0.1xx-rc2/License.txt for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace Analyzer.Utilities
{
	using static CategorizedAnalyzerConfigOptionsExtensions;

	/// <summary>
	/// Analyzer configuration options that are parsed into general and specific configuration options.
	///
	/// <para><strong>.EditorConfig</strong> format:</para>
	/// <list type="number">
	/// <item><description>General configuration option:
	/// <list type="number">
	/// <item><description><c>dotnet_code_quality.<em>OptionName</em> = <em>OptionValue</em></c></description></item>
	/// </list>
	/// </description></item>
	/// <item><description>Specific configuration option:
	/// <list type="number">
	/// <item><description><c>dotnet_code_quality.<em>RuleId</em>.<em>OptionName</em> = <em>OptionValue</em></c></description></item>
	/// <item><description><c>dotnet_code_quality.<em>RuleCategory</em>.<em>OptionName</em> = <em>OptionValue</em></c></description></item>
	/// </list>
	/// </description></item>
	/// </list>
	///
	/// <para><strong>.EditorConfig</strong> examples to configure API surface analyzed by analyzers:</para>
	/// <list type="number">
	/// <item><description>General configuration option:
	/// <list type="number">
	/// <item><description><c>dotnet_code_quality.api_surface = all</c></description></item>
	/// </list>
	/// </description></item>
	/// <item><description>Specific configuration option:
	/// <list type="number">
	/// <item><description><c>dotnet_code_quality.CA1040.api_surface = public, internal</c></description></item>
	/// <item><description><c>dotnet_code_quality.Naming.api_surface = public</c></description></item>
	/// </list>
	/// </description></item>
	/// </list>
	///
	/// <para>See SymbolVisibilityGroup for allowed symbol visibility value combinations.</para>
	/// </summary>
	internal interface ICategorizedAnalyzerConfigOptions
	{
		bool IsEmpty { get; }

		T GetOptionValue<T>(string optionName, SyntaxTree? tree, DiagnosticDescriptor? rule, TryParseValue<T> tryParseValue, T defaultValue, OptionKind kind = OptionKind.DotnetCodeQuality);
	}

	internal static class CategorizedAnalyzerConfigOptionsExtensions
	{
		public delegate bool TryParseValue<T>(string value, [MaybeNullWhen(returnValue: false)] out T parsedValue);
	}
}
