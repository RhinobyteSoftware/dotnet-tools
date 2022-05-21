using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rhinobyte.CodeAnalysis.NetAnalyzers.Tests.TestCaseFiles;

public partial class ExampleClassWithIncorrectAlphabeticalOrdering
{
	public static readonly TimeSpan TimeSpanConstant = TimeSpan.FromMinutes(1);
	public const string ConstantOne = nameof(ConstantOne);

	private readonly string _fieldA;
	private bool _fieldE;
	private readonly int _fieldB;
	private string _fieldZ;

	public ExampleClassWithIncorrectAlphabeticalOrdering()
	{

	}

	public ExampleClassWithIncorrectAlphabeticalOrdering(bool isEnabled)
	{
		IsEnabled = isEnabled;
	}

	public bool IsNotValid { get; set; }

	public bool IsEnabled { get; set; }

	public Task<bool> SomethingElseAsync(CancellationToken cancellationToken)
	{
		return Task.FromResult(true);
	}

	public static async Task DoSomethingAsync(CancellationToken cancellationToken)
	{

	}

	public string OutOfOrderProperty { get; set; }
}

public partial class ExampleClassWithIncorrectAlphabeticalOrdering
{
	public const string PartialValueOne = nameof(PartialValueOne);
	public const string PartialValueTwo = nameof(PartialValueTwo);

	private readonly string _partialFieldOne;
	private bool _partialFieldTwo;

	public ExampleClassWithIncorrectAlphabeticalOrdering(bool isEnabled, bool isNotValid)
	{
		IsEnabled = isEnabled;
		IsNotValid = isNotValid;
	}

	public string PartialPropertyOne { get; set; }
	public string PartialPropertyTwo { get; set; }

	public int PartialMethod1() => 1;
	public int PartialMethod2() => 2;
}

public partial class ExampleClassWithIncorrectAlphabeticalOrdering
{
	public const string ConstantTwo = nameof(ConstantTwo);
	public const string ConstantThree = nameof(ConstantThree);

	private readonly string _aValue;
	private bool _zValue;

	public ExampleClassWithIncorrectAlphabeticalOrdering(string aValue, bool zValue, string aProperty, string zProperty)
	{
		_aValue = aValue;
		_zValue = zValue;
		AProperty = aProperty;
		ZProperty = zProperty;
	}

	public string ZProperty { get; set; }
	public string AProperty { get; set; }

	public string ZMethod() => "Z";
	public string AMethod() => "A";
}
