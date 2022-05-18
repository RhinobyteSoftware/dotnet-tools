using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Linq;
using System.Reflection;

namespace Rhinobyte.CodeAnalysis.NetAnalyzers.Utilities;

public static partial class AnalyzerOptionsExtensions
{
	// *sigh* Why can't Microsoft make anything public so it's extensible?
	// They have no problem making that work for the EFCore 'internal' apis
	// Instead I've got to resort to these damn reflection hacks to consume them

	private static MethodInfo? _getOptionValueExtensionMethod;
	private static MethodInfo? _getOrComputeCategorizedAnalyzerConfigOptionsMethod;
	private static bool _hasTriedFindingGetOptionValueViaReflection;
	private static bool _hasTriedFindingGetOrComputeViaReflection;

	internal static MethodInfo? FindGetOptionValueMethod(object categorizedOptions)
	{
		if (_hasTriedFindingGetOptionValueViaReflection)
			return _getOptionValueExtensionMethod;

		try
		{
			var categorizedOptionsType = categorizedOptions.GetType();
			var getOptionValueMethods = categorizedOptionsType
				.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
				.Where(methodInfo => methodInfo.Name.StartsWith("GetOptionValue", StringComparison.Ordinal))
				.ToArray();

			_getOptionValueExtensionMethod = getOptionValueMethods.FirstOrDefault();
		}
#pragma warning disable CA1031 // Do not catch general exception types
		catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
		{

		}

		_hasTriedFindingGetOptionValueViaReflection = true;
		return _getOptionValueExtensionMethod;
	}

	internal static MethodInfo? FindGetOrComputeMethod()
	{
		if (_hasTriedFindingGetOrComputeViaReflection)
			return _getOrComputeCategorizedAnalyzerConfigOptionsMethod;

		try
		{
			var assembly = typeof(AnalyzerOptions).Assembly;
			var assemblyTypes = assembly.GetTypes();

			var extensionType = assemblyTypes.FirstOrDefault(discoveredType =>
			{
				var typeMethods = discoveredType.GetMethods(BindingFlags.NonPublic | BindingFlags.Static);
				return typeMethods.Any(method => method.Name.IndexOf("GetOrComputeCategorizedAnalyzerConfigOptions", StringComparison.Ordinal) > -1);
			});

			extensionType ??= assembly.GetType("Microsoft.CodeAnalysis.Diagnostics.AnalyzerOptionsExtensions");
			extensionType ??= assembly.GetTypes().FirstOrDefault(type => type.Name.IndexOf("AnalyzerOptionsExtensions", StringComparison.Ordinal) > -1);

			var methods = extensionType.GetMethods(BindingFlags.NonPublic | BindingFlags.Static);
			var methodInfo = methods.FirstOrDefault(method => method.Name.IndexOf("GetOrComputeCategorizedAnalyzerConfigOptions", StringComparison.Ordinal) > -1);
			_getOrComputeCategorizedAnalyzerConfigOptionsMethod = methodInfo;
		}
#pragma warning disable CA1031 // Do not catch general exception types
		catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
		{

		}

		_hasTriedFindingGetOrComputeViaReflection = true;
		return _getOrComputeCategorizedAnalyzerConfigOptionsMethod;
	}

	public static T GetOptionValue<T>(
		this AnalyzerOptions analyzerOptions,
		string optionName,
		Compilation compilation,
		DiagnosticDescriptor? rule,
		SyntaxTree? tree,
		TryParseValue<T> tryParseValue,
		T defaultValue)
	{
		var getOrComputeMethod = FindGetOrComputeMethod();
		if (getOrComputeMethod is null)
			return defaultValue;

		var categorizedOptions = getOrComputeMethod.Invoke(null, new object[] { analyzerOptions, compilation });
		if (categorizedOptions is null)
			return defaultValue;


		var getOptionValueGenericMethod = FindGetOptionValueMethod(categorizedOptions);
		if (getOptionValueGenericMethod is null)
			return defaultValue;

		var methodToInvoke = getOptionValueGenericMethod.MakeGenericMethod(typeof(T));
		var getOptionValueParams = new object?[] { /** optionName **/ optionName, /** syntaxTree **/ tree, /** rule **/ rule, tryParseValue, /** defaultValue **/ defaultValue };
		var optionValue = methodToInvoke?.Invoke(analyzerOptions, getOptionValueParams);
		if (optionValue is T parsedValue)
		{
			return parsedValue;
		}

		return defaultValue;
	}

	public static string GetStringOptionValue(
		this AnalyzerOptions analyzerOptions,
		string optionName,
		Compilation compilation,
		DiagnosticDescriptor? rule,
		SyntaxTree? tree,
		string defaultValue)
	{
		return analyzerOptions.GetOptionValue<string>(optionName, compilation, rule, tree, TryParseStringValue, defaultValue);
	}

	public static bool TryParseStringValue(string value, out string parsedValue)
	{
		parsedValue = value;
		return true;
	}

	public delegate bool TryParseValue<T>(string value, out T parsedValue);
}
