using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Shim IsExternalInit for .NET Standard 2.0
namespace System.Runtime.CompilerServices
{
	using global::System.Diagnostics;
	using global::System.Diagnostics.CodeAnalysis;

	[ExcludeFromCodeCoverage, DebuggerNonUserCode]
	internal static class IsExternalInit
	{
	}
}

namespace Rhinobyte.CodeAnalysis.NetAnalyzers.Tests.TestCaseFiles
{
	public record ExampleRecord1(string Alpha, string Delta, string Charlie, string Bravo);

	public record ExampleRecord2
	{
		public string Alpha { get; init; }
		public string Delta { get; init; }
		public string Charlie { get; init; }
		public string Bravo { get; init; }
	}

	public record class ExampleRecord3
	{
		public string Alpha { get; init; }
		public string Delta { get; init; }
		public string Charlie { get; init; }
		public string Bravo { get; init; }
	}

	public readonly record struct ExampleRecord4(string Alpha, string Delta, string Charlie, string Bravo);

	public readonly record struct ExampleRecord5
	{
		public string Alpha { get; init; }
		public string Delta { get; init; }
		public string Charlie { get; init; }
		public string Bravo { get; init; }
	}

	public class WrapperClass
	{
		public record ExampleRecord6(string Alpha, string Delta, string Charlie, string Bravo);

		public readonly record struct ExampleRecord7
		{
			public string Alpha { get; init; }
			public string Delta { get; init; }
			public string Charlie { get; init; }
			public string Bravo { get; init; }
		}
	}
}
