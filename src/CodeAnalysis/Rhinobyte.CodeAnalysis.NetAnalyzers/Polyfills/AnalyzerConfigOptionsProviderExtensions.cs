// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See https://github.com/dotnet/roslyn-analyzers/blob/release/6.0.1xx-rc2/License.txt for license information.

#if CODEANALYSIS_V3_OR_BETTER

using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Analyzer.Utilities
{
	internal static class AnalyzerConfigOptionsProviderExtensions
	{
		public static bool IsEmpty(this AnalyzerConfigOptionsProvider analyzerConfigOptionsProvider)
		{
			// Reflection based optimization for empty analyzer config options.
			// Ideally 'AnalyzerConfigOptionsProvider.IsEmpty' would be exposed in the API.
			const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
			return analyzerConfigOptionsProvider.GetType().GetField("_treeDict", flags)?.GetValue(analyzerConfigOptionsProvider) is ImmutableDictionary<object, AnalyzerConfigOptions> perTreeOptionsMap
				&& perTreeOptionsMap.IsEmpty;
		}
	}
}

#endif
