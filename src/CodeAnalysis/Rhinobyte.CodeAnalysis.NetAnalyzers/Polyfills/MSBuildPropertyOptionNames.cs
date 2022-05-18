// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the MIT license.  See https://github.com/dotnet/roslyn-analyzers/blob/release/6.0.1xx-rc2/License.txt for license information.

using System.Diagnostics;

using System.Linq;

namespace Analyzer.Utilities
{
	/// <summary>
	/// MSBuild property names that are required to be threaded as analyzer config options.
	/// </summary>
	internal static class MSBuildPropertyOptionNames
	{
		public const string TargetFramework = nameof(TargetFramework);
		public const string TargetPlatformMinVersion = nameof(TargetPlatformMinVersion);
		public const string UsingMicrosoftNETSdkWeb = nameof(UsingMicrosoftNETSdkWeb);
		public const string ProjectTypeGuids = nameof(ProjectTypeGuids);
		public const string InvariantGlobalization = nameof(InvariantGlobalization);
		public const string PlatformNeutralAssembly = nameof(PlatformNeutralAssembly);
	}

	internal static class MSBuildPropertyOptionNamesHelpers
	{
		[Conditional("DEBUG")]
		public static void VerifySupportedPropertyOptionName(string propertyOptionName)
		{
			Debug.Assert(typeof(MSBuildPropertyOptionNames).GetFields().Single(f => f.Name == propertyOptionName) != null);
		}
	}
}
