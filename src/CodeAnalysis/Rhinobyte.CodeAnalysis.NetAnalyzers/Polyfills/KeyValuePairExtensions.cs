﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See https://github.com/dotnet/roslyn-analyzers/blob/release/6.0.1xx-rc2/License.txt for license information.

using System.Collections.Generic;

namespace Analyzer.Utilities.Extensions
{
	internal static class KeyValuePairExtensions
	{
		public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> pair, out TKey key, out TValue value)
		{
			key = pair.Key;
			value = pair.Value;
		}

		public static KeyValuePair<TKey?, TValue?> AsNullable<TKey, TValue>(this KeyValuePair<TKey, TValue> pair)
		{
			// This conversion is safe
			return pair!;
		}
	}
}
