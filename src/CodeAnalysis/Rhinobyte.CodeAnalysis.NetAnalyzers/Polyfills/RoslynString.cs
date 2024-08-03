// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See https://github.com/dotnet/roslyn-analyzers/blob/release/6.0.1xx-rc2/License.txt for license information.

using System.Diagnostics.CodeAnalysis;

namespace Analyzer.Utilities
{
	internal static class RoslynString
	{
		/// <inheritdoc cref="string.IsNullOrEmpty(string)"/>
		public static bool IsNullOrEmpty([NotNullWhen(returnValue: false)] string? value)
			=> string.IsNullOrEmpty(value);

#if !NET20
		/// <inheritdoc cref="string.IsNullOrWhiteSpace(string)"/>
		public static bool IsNullOrWhiteSpace([NotNullWhen(returnValue: false)] string? value)
			=> string.IsNullOrWhiteSpace(value);
#endif
	}
}
