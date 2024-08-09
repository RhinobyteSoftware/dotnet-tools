using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Rhinobyte.CodeAnalysis.NetAnalyzers.Tests.TestCaseFiles;

public partial class PartialClassWithPrimaryConstructor(
	string primaryConstructorParam1,
	string primaryConstructorParam2)
{
	private readonly int _field1;

	public static readonly TimeSpan TimeSpanConstant = TimeSpan.FromMinutes(1);

	public static bool StaticProperty { get; } = true;

	public PartialClassWithPrimaryConstructor()
		: this(string.Empty, string.Empty)
	{

	}
	public const string ConstantOne = nameof(ConstantOne);

	//[GeneratedRegex("\\s+")]
	private static partial Regex WhitespacePattern();

	public bool PropertyA { get; set; }
}

partial class PartialClassWithPrimaryConstructor
{
	public string PropertyB { get; set; }

	private readonly int _field2;

	public static async Task DoSomethingAsync(CancellationToken cancellationToken)
	{
		await Task.Delay(1_000, cancellationToken);
	}

	private static partial Regex WhitespacePattern()
	{
		return new Regex("\\s+");
	}
}
