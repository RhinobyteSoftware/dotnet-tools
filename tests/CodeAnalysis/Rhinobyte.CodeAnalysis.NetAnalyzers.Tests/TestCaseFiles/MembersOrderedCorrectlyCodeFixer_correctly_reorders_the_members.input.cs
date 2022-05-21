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

	public ExampleClassWithIncorrectGroupOrdering()
    {

    }

	public bool IsEnabled { get; set; }

	private readonly int _outOfOrderedField1;

	public ExampleClassWithIncorrectGroupOrdering(bool isEnabled)
	{
		IsEnabled = isEnabled;
	}

	public static async Task DoSomethingAsync(CancellationToken cancellationToken)
    {

    }

	public string OutOfOrderProperty { get; set; }

	private int _outOfOrderedField2;

	public Task<bool> SomethingElseAsync(CancellationToken cancellationToken)
    {
		return Task.FromResult(true);
    }
}
