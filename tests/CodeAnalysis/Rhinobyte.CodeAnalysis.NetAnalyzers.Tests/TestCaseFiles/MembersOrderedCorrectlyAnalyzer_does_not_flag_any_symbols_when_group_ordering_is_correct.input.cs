using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Some.Name.Space;

public class ExampleClass
{
	private const string SomeConstant = nameof(SomeConstant);

	private readonly string _fieldOne;
	private readonly int _fieldTwo;

	public ExampleClass()
	{
		_fieldOne = string.Empty;
		_fieldTwo = 10;
	}

	public ExampleClass(string fieldOne, int fieldTwo)
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
