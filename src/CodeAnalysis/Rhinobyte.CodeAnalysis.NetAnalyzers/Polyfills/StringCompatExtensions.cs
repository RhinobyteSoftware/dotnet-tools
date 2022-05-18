// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See https://github.com/dotnet/roslyn-analyzers/blob/release/6.0.1xx-rc2/License.txt for license information.

#if !NETCOREAPP

#pragma warning disable IDE0161 // Convert to file-scoped namespace
namespace System
#pragma warning restore IDE0161 // Convert to file-scoped namespace
{
	internal static class StringCompatExtensions
	{
		public static bool Contains(this string str, string value, StringComparison comparisonType)
		{
			return str.IndexOf(value, comparisonType) >= 0;
		}

		public static string Replace(this string str, string oldValue, string? newValue, StringComparison comparisonType)
		{
			if (comparisonType != StringComparison.Ordinal)
				throw new NotSupportedException();

			return str.Replace(oldValue, newValue);
		}
	}
}

#endif
