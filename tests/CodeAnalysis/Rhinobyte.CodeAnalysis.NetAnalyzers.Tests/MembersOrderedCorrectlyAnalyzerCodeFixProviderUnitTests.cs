using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;
using VerifyCS = Rhinobyte.CodeAnalysis.NetAnalyzers.Tests.CSharpCodeFixVerifier<
	Rhinobyte.CodeAnalysis.NetAnalyzers.MembersOrderedCorrectlyAnalyzer,
	Rhinobyte.CodeAnalysis.NetAnalyzers.MembersOrderedCorrectlyAnalyzerCodeFixProvider>;

namespace Rhinobyte.CodeAnalysis.NetAnalyzers.Tests;

[TestClass]
public class MembersOrderedCorrectlyAnalyzerCodeFixProviderUnitTests
{
	public CancellationToken CancellationTokenForTest => TestContext?.CancellationTokenSource?.Token ?? default;

	public TestContext TestContext { get; set; }

	//Diagnostic and CodeFix both triggered and checked for
	[TestMethod]
	public async Task MembersOrderedCorrectlyCodeFixer_correctly_reorders_the_members()
	{
		var testContent = await TestHelper.GetTestInputFileAsync(CancellationTokenForTest);
		var codeFixResult = await TestHelper.GetTestCodeFixResultFileAsync(CancellationTokenForTest);

		var expectedDiagnosticResults = new DiagnosticResult[]
		{
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0001).WithSpan(23, 23, 23, 42).WithArguments("_outOfOrderedField1"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0001).WithSpan(25, 9, 25, 47).WithArguments(".ctor"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0001).WithSpan(35, 16, 35, 34).WithArguments("OutOfOrderProperty"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0001).WithSpan(37, 14, 37, 33).WithArguments("_outOfOrderedField2"),
		};

		await VerifyCS.VerifyCodeFixAsync(testContent, expectedDiagnosticResults, codeFixResult);
	}

	//Diagnostic and CodeFix both triggered and checked for
	[TestMethod]
	public async Task MembersOrderedCorrectlyCodeFixer_correctly_reorders_the_members_in_partial_classes()
	{
		var testContent = await TestHelper.GetTestInputFileAsync(CancellationTokenForTest);
		var codeFixResult = await TestHelper.GetTestCodeFixResultFileAsync(CancellationTokenForTest);

		var expectedDiagnosticResults = new DiagnosticResult[]
		{
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0001).WithSpan(51, 20, 51, 38).WithArguments("OutOfOrderConstant"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0001).WithSpan(53, 23, 53, 40).WithArguments("_outOfOrderField1"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0001).WithSpan(56, 9, 56, 47).WithArguments(".ctor"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0001).WithSpan(61, 16, 61, 34).WithArguments("OutOfOrderProperty"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0001).WithSpan(64, 9, 64, 47).WithArguments(".cctor"),
		};

		await VerifyCS.VerifyCodeFixAsync(testContent, expectedDiagnosticResults, codeFixResult);
	}
}
