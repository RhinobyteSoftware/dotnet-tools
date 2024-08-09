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
	public const string ConstantOne = nameof(ConstantOne);

	public static readonly TimeSpan TimeSpanConstant = TimeSpan.FromMinutes(1);

	public static bool StaticProperty { get; } = true;

    private readonly int _field1;

	public PartialClassWithPrimaryConstructor()
		: this(string.Empty, string.Empty)
	{

	}

	public bool PropertyA { get; set; }

	//[GeneratedRegex("\\s+")]
	private static partial Regex WhitespacePattern();
}

partial class PartialClassWithPrimaryConstructor
{

	private readonly int _field2;

    public string PropertyB { get; set; }

	public static async Task DoSomethingAsync(CancellationToken cancellationToken)
	{
		await Task.Delay(1_000, cancellationToken);
	}

	private static partial Regex WhitespacePattern()
	{
		return new Regex("\\s+");
	}
}
