using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Some.Name.Space;

public partial class ExamplePartialClass
{
	private const string SomeConstant = nameof(SomeConstant);

	private readonly string _fieldOne;
	private readonly int _fieldTwo;

	public ExamplePartialClass()
	{
		_fieldOne = string.Empty;
		_fieldTwo = 10;
	}

	public ExamplePartialClass(string fieldOne, int fieldTwo)
	{
		_fieldOne = fieldOne;
		_fieldTwo = fieldTwo;
	}

	public string PropertyOne { get; set; }

	public string PropertyTwo { get; set; }

	public void DoSomething()
	{
	}

	public async Task DoSomethingAsync(CancellationToken cancellationToken)
	{
	}

	public async Task SomethingElseAsync(CancellationToken cancellationToken)
	{
	}
}

public partial class ExamplePartialClass
{
	private const string AnotherConstant = nameof(AnotherConstant);

	private readonly decimal _alphaField;
	private readonly decimal _zetaField;

	protected ExamplePartialClass(
		decimal alphaField,
		string fieldOne,
		int fieldTwo,
		decimal zetaField)
    {
		_alphaField = alphaField;
		_fieldOne = fieldOne;
		_fieldTwo = fieldTwo;
		_zetaField = zetaField;
    }

	public DateTimeOffset Beginning { get; set; }

	public DateTimeOffset VeryEnd { get; set; }

	private void AnalyzeSomething()
	{
	}

	public IEnumerable<object> EnumeratorMethod()
    {
		yield return new object();
		yield return new object();
    }

	public async Task MiddleAsync(CancellationToken cancellationToken)
	{
	}

	public async Task VerifyAsync(CancellationToken cancellationToken)
	{
	}
}
