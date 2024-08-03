﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See https://github.com/dotnet/roslyn-analyzers/blob/release/6.0.1xx-rc2/License.txt for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

using System.Linq;

namespace Analyzer.Utilities
{
	/// <summary>
	/// MSBuild item names that are required to be threaded as analyzer config options.
	/// The analyzer config option will have the following key/value:
	/// - Key: Item name prefixed with an '_' and suffixed with a 'List' to reduce chances of conflicts with any existing project property.
	/// - Value: Concatenated item metadata values, separated by a ',' character. See https://github.com/dotnet/sdk/issues/12706#issuecomment-668219422 for details.
	/// </summary>
	internal static class MSBuildItemOptionNames
	{
		public const string SupportedPlatform = nameof(SupportedPlatform);
	}

	internal static class MSBuildItemOptionNamesHelpers
	{
		public const char ValuesSeparator = ',';
		private static readonly char[] s_itemMetadataValuesSeparators = [ValuesSeparator];

		public static string GetPropertyNameForItemOptionName(string itemOptionName)
		{
			VerifySupportedItemOptionName(itemOptionName);
			return $"_{itemOptionName}List";
		}

		[Conditional("DEBUG")]
		public static void VerifySupportedItemOptionName(string itemOptionName)
		{
            Debug.Assert(typeof(MSBuildItemOptionNames).GetFields().Single(f => f.Name == itemOptionName) != null);
		}

		public static ImmutableArray<string> ParseItemOptionValue(string? itemOptionValue)
		{
			return itemOptionValue is null
				? []
				: ProduceTrimmedArray(itemOptionValue).ToImmutableArray();
		}

		private static IEnumerable<string> ProduceTrimmedArray(string itemOptionValue)
		{
			foreach (var platform in itemOptionValue.Split(s_itemMetadataValuesSeparators, StringSplitOptions.RemoveEmptyEntries))
			{
				yield return platform.Trim();
			}
		}
	}
}
