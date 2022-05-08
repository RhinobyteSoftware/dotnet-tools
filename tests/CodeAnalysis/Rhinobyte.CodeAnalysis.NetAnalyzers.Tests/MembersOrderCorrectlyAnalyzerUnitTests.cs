using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using VerifyCS = Rhinobyte.CodeAnalysis.NetAnalyzers.Tests.CSharpCodeFixVerifier<
	Rhinobyte.CodeAnalysis.NetAnalyzers.MembersOrderedCorrectlyAnalyzer,
	Rhinobyte.CodeAnalysis.NetAnalyzers.MembersOrderedCorrectlyAnalyzerCodeFixProvider>;

namespace Rhinobyte.CodeAnalysis.NetAnalyzers.Tests;

[TestClass]
public class MembersOrderCorrectlyAnalyzerUnitTests
{
	//No diagnostics expected to show up
	[TestMethod]
	public async Task TestMethod1()
	{
		var test = @"
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
	private readonly int	_fieldTwo;

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
}";

		await VerifyCS.VerifyAnalyzerAsync(test);
	}

	//Diagnostic and CodeFix both triggered and checked for
	[TestMethod]
	public async Task TestMethod2()
	{
		var test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    class {|#0:TypeName|}
    {   
    }
}";

		var fixtest = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    class TYPENAME
    {
    }
}";

		var expected = VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.DiagnosticId).WithLocation(0).WithArguments("TypeName");
		await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
	}
}
