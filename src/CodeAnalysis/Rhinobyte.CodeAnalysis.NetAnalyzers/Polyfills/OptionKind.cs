// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See https://github.com/dotnet/roslyn-analyzers/blob/release/6.0.1xx-rc2/License.txt for license information.

namespace Analyzer.Utilities
{
	/// <summary>
	/// Kind of option to fetch from <see cref="ICategorizedAnalyzerConfigOptions"/>.
	/// </summary>
	internal enum OptionKind
	{
		/// <summary>
		/// Option prefixed with <c>dotnet_code_quality.</c>.
		/// <para>Used for custom analyzer config options for analyzers in this repo.</para>
		/// </summary>
		DotnetCodeQuality,

		/// <summary>
		/// Option prefixed with <c>build_property.</c>.
		/// <para>Used for options generated for MSBuild properties.</para>
		/// </summary>
		BuildProperty,
	}
}
