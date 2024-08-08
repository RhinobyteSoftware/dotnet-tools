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

	//Diagnostic and CodeFix both triggered and checked for
	[TestMethod]
	public async Task MembersOrderedCorrectlyCodeFixer_correctly_reorders_the_members_when_not_alphabetical()
	{
		var testContent = await TestHelper.GetTestInputFileAsync(CancellationTokenForTest);
		var codeFixResult = await TestHelper.GetTestCodeFixResultFileAsync(CancellationTokenForTest);

		var expectedDiagnosticResults = new DiagnosticResult[]
		{
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0002).WithSpan(14, 22, 14, 31).WithArguments("ConstantD"),
			// /0/Test0.cs(16,22): info RBCS0002: Member 'ConstantB' is not ordered alphabetically
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0002).WithSpan(16, 22, 16, 31).WithArguments("ConstantB"),
			// /0/Test0.cs(21,21): info RBCS0002: Member 'StaticPropertyD' is not ordered alphabetically
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0002).WithSpan(21, 21, 21, 36).WithArguments("StaticPropertyD"),
			// /0/Test0.cs(23,21): info RBCS0002: Member 'StaticPropertyB' is not ordered alphabetically
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0002).WithSpan(23, 21, 23, 36).WithArguments("StaticPropertyB"),
			// /0/Test0.cs(33,13): info RBCS0002: Member 'PropertyD' is not ordered alphabetically
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0002).WithSpan(33, 13, 33, 22).WithArguments("PropertyD"),
			// /0/Test0.cs(35,13): info RBCS0002: Member 'PropertyB' is not ordered alphabetically
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0002).WithSpan(35, 13, 35, 22).WithArguments("PropertyB"),
			// /0/Test0.cs(46,27): info RBCS0002: Member 'MethodD' is not ordered alphabetically
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0002).WithSpan(46, 27, 46, 34).WithArguments("MethodD"),
			// /0/Test0.cs(53,23): info RBCS0002: Member 'MethodB' is not ordered alphabetically
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0002).WithSpan(53, 23, 53, 30).WithArguments("MethodB"),
		};

		await VerifyCS.VerifyCodeFixAsync(testContent, expectedDiagnosticResults, codeFixResult);
		//await VerifyCS.VerifyCodeFixAsync(testContent, codeFixResult);
	}

	//Diagnostic and CodeFix both triggered and checked for
	[TestMethod]
	public async Task MembersOrderedCorrectlyCodeFixer_correctly_reorders_the_members_when_not_alphabetical_in_partial_classes()
	{
		var testContent = await TestHelper.GetTestInputFileAsync(CancellationTokenForTest);
		var codeFixResult = await TestHelper.GetTestCodeFixResultFileAsync(CancellationTokenForTest);

		var expectedDiagnosticResults = new DiagnosticResult[]
		{
			// /0/Test0.cs(33,22): info RBCS0002: Member 'ConstantD' is not ordered alphabetically
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0002).WithSpan(33, 22, 33, 31).WithArguments("ConstantD"),
			// /0/Test0.cs(35,22): info RBCS0002: Member 'ConstantB' is not ordered alphabetically
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0002).WithSpan(35, 22, 35, 31).WithArguments("ConstantB"),
			// /0/Test0.cs(39,21): info RBCS0002: Member 'StaticPropertyD' is not ordered alphabetically
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0002).WithSpan(39, 21, 39, 36).WithArguments("StaticPropertyD"),
			// /0/Test0.cs(41,21): info RBCS0002: Member 'StaticPropertyB' is not ordered alphabetically
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0002).WithSpan(41, 21, 41, 36).WithArguments("StaticPropertyB"),
			// /0/Test0.cs(50,13): info RBCS0002: Member 'PropertyD' is not ordered alphabetically
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0002).WithSpan(50, 13, 50, 22).WithArguments("PropertyD"),
			// /0/Test0.cs(52,13): info RBCS0002: Member 'PropertyB' is not ordered alphabetically
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0002).WithSpan(52, 13, 52, 22).WithArguments("PropertyB"),
			// /0/Test0.cs(58,27): info RBCS0002: Member 'MethodD' is not ordered alphabetically
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0002).WithSpan(58, 27, 58, 34).WithArguments("MethodD"),
			// /0/Test0.cs(65,23): info RBCS0002: Member 'MethodB' is not ordered alphabetically
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0002).WithSpan(65, 23, 65, 30).WithArguments("MethodB"),
		};

		// TODO: Figure out why their test runner is also adding spaces, only to the lines where I adjust the whitespace, on the output when applying the fixes

		await VerifyCS.VerifyCodeFixAsync(testContent, expectedDiagnosticResults, codeFixResult);
		//await VerifyCS.VerifyCodeFixAsync(testContent, codeFixResult);
	}


	[TestMethod]
	public async Task MembersOrderedCorrectlyCodeFixer_correctly_reorders_with_primary_constructor()
	{
		var testContent = await TestHelper.GetTestInputFileAsync(CancellationTokenForTest);
		var codeFixResult = await TestHelper.GetTestCodeFixResultFileAsync(CancellationTokenForTest);

		var expectedDiagnosticResults = new DiagnosticResult[]
		{
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0001).WithSpan(26, 23, 26, 42).WithArguments("_outOfOrderedField1"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0001).WithSpan(28, 9, 28, 47).WithArguments(".ctor"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0001).WithSpan(39, 16, 39, 34).WithArguments("OutOfOrderProperty"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0001).WithSpan(41, 14, 41, 33).WithArguments("_outOfOrderedField2"),
		};

		await VerifyCS.VerifyCodeFixAsync(testContent, expectedDiagnosticResults, codeFixResult);
	}

	[TestMethod]
	public async Task MembersOrderedCorrectlyCodeFixer_reorders_enum_members()
	{
		var testContent = await TestHelper.GetTestInputFileAsync(CancellationTokenForTest);
		var codeFixResult = await TestHelper.GetTestCodeFixResultFileAsync(CancellationTokenForTest);

		var expectedDiagnosticResults = new DiagnosticResult[]
		{
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0004).WithSpan(13, 2, 13, 7).WithArguments("Bravo"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0004).WithSpan(15, 2, 15, 7).WithArguments("Delta"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0004).WithSpan(34, 2, 34, 7).WithArguments("Bravo"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0004).WithSpan(44, 2, 44, 7).WithArguments("Delta"),
		};

		await VerifyCS.VerifyCodeFixAsync(testContent, expectedDiagnosticResults, codeFixResult);
	}
}
