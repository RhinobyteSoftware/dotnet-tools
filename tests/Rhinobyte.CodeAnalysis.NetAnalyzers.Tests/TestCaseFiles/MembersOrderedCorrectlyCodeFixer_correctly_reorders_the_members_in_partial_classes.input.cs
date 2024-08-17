using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rhinobyte.CodeAnalysis.NetAnalyzers.Tests.TestCaseFiles;

public partial class ExampleClassWithIncorrectGroupOrdering
{
	public const string ConstantOne = nameof(ConstantOne);
	public static readonly TimeSpan TimeSpanConstant = TimeSpan.FromMinutes(1);

	public static bool StaticProperty { get; private set; }

	public ExampleClassWithIncorrectGroupOrdering()
	{

	}

	public bool IsEnabled { get; set; }

	public static async Task DoSomethingAsync(CancellationToken cancellationToken)
	{

	}

	public Task<bool> SomethingElseAsync(CancellationToken cancellationToken)
	{
		return Task.FromResult(true);
	}
}

public partial class ExampleClassWithIncorrectGroupOrdering
{
	public const string OtherConstant = nameof(ConstantOne);
	public static readonly TimeSpan OtherTimeSpanConstant = TimeSpan.FromMinutes(1);

	public static bool StaticPropertyTwo { get; private set; }

	public ExampleClassWithIncorrectGroupOrdering(bool isEnabled)
	{
		IsEnabled = isEnabled;
	}

	public void OtherMethod()
	{

	}

	private const int OutOfOrderConstant = 0;

	private readonly int _outOfOrderField1;

	// Out of order constructor
	public ExampleClassWithIncorrectGroupOrdering(int outOfOrderField)
	{
		_outOfOrderField1 = outOfOrderField;
	}

	public string OutOfOrderProperty { get; set; }

	// Out of order static constructor
	static ExampleClassWithIncorrectGroupOrdering()
	{
		StaticProperty = true;
		StaticPropertyTwo = false;
	}

	public static async Task OtherMethodAsync(CancellationToken cancellationToken)
	{

	}

	public Task<bool> VeryLastMethodAsync(CancellationToken cancellationToken)
	{
		return Task.FromResult(true);
	}
}
