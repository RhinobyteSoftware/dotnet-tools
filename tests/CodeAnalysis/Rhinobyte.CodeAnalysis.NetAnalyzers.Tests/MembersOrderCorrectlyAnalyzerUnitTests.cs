using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;
using VerifyCS = Rhinobyte.CodeAnalysis.NetAnalyzers.Tests.CSharpCodeFixVerifier<
	Rhinobyte.CodeAnalysis.NetAnalyzers.MembersOrderedCorrectlyAnalyzer,
	Rhinobyte.CodeAnalysis.NetAnalyzers.MembersOrderedCorrectlyAnalyzerCodeFixProvider>;

namespace Rhinobyte.CodeAnalysis.NetAnalyzers.Tests;

[TestClass]
public class MembersOrderCorrectlyAnalyzerUnitTests
{
	public CancellationToken CancellationTokenForTest => TestContext?.CancellationTokenSource?.Token ?? default;

	public TestContext TestContext { get; set; }


	[TestMethod]
	public async Task MembersOrderedCorrectlyAnalyzer_correctly_flags_symbols_that_arent_ordered_correctly_by_group()
	{
		var testContent = await TestHelper.GetTestInputFileAsync(CancellationTokenForTest);

		var expectedDiagnosticResults = new DiagnosticResult[]
		{
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0001).WithSpan(23, 23, 23, 42).WithArguments("_outOfOrderedField1"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0001).WithSpan(25, 9, 25, 47).WithArguments(".ctor"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0001).WithSpan(35, 16, 35, 34).WithArguments("OutOfOrderProperty"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0001).WithSpan(37, 14, 37, 33).WithArguments("_outOfOrderedField2"),
		};

		await VerifyCS.VerifyAnalyzerAsync(testContent, expectedDiagnosticResults);
	}

	[TestMethod]
	public async Task MembersOrderedCorrectlyAnalyzer_correctly_flags_symbols_that_arent_ordered_correctly_by_group_in_partial_classes()
	{
		var testContent = await TestHelper.GetTestInputFileAsync(CancellationTokenForTest);

		var expectedDiagnosticResults = new DiagnosticResult[]
		{
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0001).WithSpan(51, 20, 51, 38).WithArguments("OutOfOrderConstant"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0001).WithSpan(53, 23, 53, 40).WithArguments("_outOfOrderField1"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0001).WithSpan(56, 9, 56, 47).WithArguments(".ctor"), // Out of order instance constructor
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0001).WithSpan(61, 16, 61, 34).WithArguments("OutOfOrderProperty"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0001).WithSpan(64, 9, 64, 47).WithArguments(".cctor"), // Out of order static constructor
		};

		await VerifyCS.VerifyAnalyzerAsync(testContent, expectedDiagnosticResults);
		//await VerifyCS.VerifyAnalyzerAsync(testContent);
	}

	//No diagnostics expected to show up
	[TestMethod]
	public async Task MembersOrderedCorrectlyAnalyzer_does_not_flag_any_symbols_when_group_ordering_is_correct()
	{
		var testContent = await TestHelper.GetTestInputFileAsync(CancellationTokenForTest);

		await VerifyCS.VerifyAnalyzerAsync(testContent);
	}

	[TestMethod]
	public async Task MembersOrderedCorrectlyAnalyzer_does_not_flag_any_symbols_when_group_ordering_is_correct_in_partial_classes()
	{
		var testContent = await TestHelper.GetTestInputFileAsync(CancellationTokenForTest);

		await VerifyCS.VerifyAnalyzerAsync(testContent);
	}

	//	//Diagnostic and CodeFix both triggered and checked for
	//	[TestMethod]
	//	public async Task TestMethod2()
	//	{
	//		var test = @"
	//using System;
	//using System.Collections.Generic;
	//using System.Linq;
	//using System.Text;
	//using System.Threading.Tasks;
	//using System.Diagnostics;

	//namespace ConsoleApplication1
	//{
	//    class {|#0:TypeName|}
	//    {   
	//    }
	//}";

	//		var fixtest = @"
	//using System;
	//using System.Collections.Generic;
	//using System.Linq;
	//using System.Text;
	//using System.Threading.Tasks;
	//using System.Diagnostics;

	//namespace ConsoleApplication1
	//{
	//    class TYPENAME
	//    {
	//    }
	//}";

	//		var expected = VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.DiagnosticId).WithLocation(0).WithArguments("TypeName");
	//		await VerifyCS.VerifyCodeFixAsync(test, expected, fixtest);
	//	}
}
