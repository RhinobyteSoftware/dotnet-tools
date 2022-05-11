using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rhinobyte.CodeAnalysis.NetAnalyzers.Tests.TestCaseFiles;

public class ExampleClassWithIncorrectGroupOrdering
{
	public const string ConstantOne = nameof(ConstantOne);
	public static readonly TimeSpan TimeSpanConstant = TimeSpan.FromMinutes(1);

	public static bool StaticProperty { get; } = true;

	private readonly int _outOfOrderedField1;

	private int _outOfOrderedField2;

	public ExampleClassWithIncorrectGroupOrdering()
    {

    }

	public ExampleClassWithIncorrectGroupOrdering(bool isEnabled)
	{
		IsEnabled = isEnabled;
	}

	public bool IsEnabled { get; set; }

	public string OutOfOrderProperty { get; set; }

	public static async Task DoSomethingAsync(CancellationToken cancellationToken)
    {

    }

	public Task<bool> SomethingElseAsync(CancellationToken cancellationToken)
    {
		return Task.FromResult(true);
    }
}
