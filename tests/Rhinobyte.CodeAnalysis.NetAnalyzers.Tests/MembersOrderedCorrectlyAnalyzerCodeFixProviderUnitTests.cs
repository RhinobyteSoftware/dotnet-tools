using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
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
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0001).WithSpan(23, 23, 23, 42).WithArguments("_outOfOrderedField1"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0001).WithSpan(25, 9, 25, 47).WithArguments(".ctor"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0001).WithSpan(35, 16, 35, 34).WithArguments("OutOfOrderProperty"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0001).WithSpan(37, 14, 37, 33).WithArguments("_outOfOrderedField2"),
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
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0001).WithSpan(51, 20, 51, 38).WithArguments("OutOfOrderConstant"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0001).WithSpan(53, 23, 53, 40).WithArguments("_outOfOrderField1"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0001).WithSpan(56, 9, 56, 47).WithArguments(".ctor"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0001).WithSpan(61, 16, 61, 34).WithArguments("OutOfOrderProperty"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0001).WithSpan(64, 9, 64, 47).WithArguments(".cctor"),
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
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0001).WithSpan(26, 23, 26, 42).WithArguments("_outOfOrderedField1"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0001).WithSpan(28, 9, 28, 47).WithArguments(".ctor"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0001).WithSpan(39, 16, 39, 34).WithArguments("OutOfOrderProperty"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0001).WithSpan(41, 14, 41, 33).WithArguments("_outOfOrderedField2"),
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

	[TestMethod]
	public async Task MembersOrderedCorrectlyCodeFixer_reorders_method_parameters()
	{
		var testContent = await TestHelper.GetTestInputFileAsync(CancellationTokenForTest);
		var codeFixResult = await TestHelper.GetTestCodeFixResultFileAsync(CancellationTokenForTest);

		var expectedDiagnosticResults = new DiagnosticResult[]
		{
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0005).WithSpan(9, 40, 11, 15).WithArguments("alpha"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0005).WithSpan(20, 35, 25, 16).WithArguments("bravo, delta"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0005).WithSpan(41, 25, 41, 46).WithArguments("alpha"),
		};

		var editorConfigSettings = new List<(string, string)>()
		{
			("/.EditorConfig", $@"root = true

[*]
dotnet_diagnostic.RBCS0001.severity = none
dotnet_diagnostic.RBCS0002.severity = none
dotnet_diagnostic.RBCS0003.severity = none
")
		};

		await VerifyCS.VerifyCodeFixAsync(testContent, expectedDiagnosticResults, codeFixResult, analyzerConfigFiles: editorConfigSettings);
	}

	[TestMethod]
	public async Task MembersOrderedCorrectlyCodeFixer_works_with_primary_constructor_partial_classes()
	{
		var testContent = await TestHelper.GetTestInputFileAsync(CancellationTokenForTest);
		var codeFixResult = await TestHelper.GetTestCodeFixResultFileAsync(CancellationTokenForTest);

		var expectedDiagnosticResults = new DiagnosticResult[]
		{
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0001).WithSpan(16, 34, 16, 50).WithArguments("TimeSpanConstant"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0001).WithSpan(18, 21, 18, 35).WithArguments("StaticProperty"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0001).WithSpan(25, 22, 25, 33).WithArguments("ConstantOne"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0001).WithSpan(30, 14, 30, 23).WithArguments("PropertyA"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0001).WithSpan(37, 23, 37, 30).WithArguments("_field2"),
		};

		await VerifyCS.VerifyCodeFixAsync(testContent, expectedDiagnosticResults, codeFixResult);
	}

	[TestMethod]
	public async Task MembersOrderedCorrectlyCodeFixer_reorders_record_members()
	{
		var testContent = await TestHelper.GetTestInputFileAsync(CancellationTokenForTest);
		var codeFixResult = await TestHelper.GetTestCodeFixResultFileAsync(CancellationTokenForTest);

		var expectedDiagnosticResults = new DiagnosticResult[]
		{
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0006).WithSpan(21, 66, 21, 73).WithArguments("Charlie"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0006).WithSpan(21, 82, 21, 87).WithArguments("Bravo"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0006).WithSpan(27, 17, 27, 24).WithArguments("Charlie"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0006).WithSpan(28, 17, 28, 22).WithArguments("Bravo"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0006).WithSpan(35, 17, 35, 24).WithArguments("Charlie"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0006).WithSpan(36, 17, 36, 22).WithArguments("Bravo"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0006).WithSpan(39, 82, 39, 89).WithArguments("Charlie"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0006).WithSpan(39, 98, 39, 103).WithArguments("Bravo"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0006).WithSpan(45, 17, 45, 24).WithArguments("Charlie"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0006).WithSpan(46, 17, 46, 22).WithArguments("Bravo"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0006).WithSpan(51, 67, 51, 74).WithArguments("Charlie"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0006).WithSpan(51, 83, 51, 88).WithArguments("Bravo"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0006).WithSpan(57, 18, 57, 25).WithArguments("Charlie"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0006).WithSpan(58, 18, 58, 23).WithArguments("Bravo"),
		};

		await VerifyCS.VerifyCodeFixAsync(testContent, expectedDiagnosticResults, codeFixResult);
	}

	[TestMethod]
	public async Task MembersOrderedCorrectlyCodeFixer_handles_casesensitivity_settings1()
	{
		var testContent = await TestHelper.GetTestInputFileAsync(CancellationTokenForTest);
		var codeFixResult = await TestHelper.GetTestCodeFixResultFileAsync(CancellationTokenForTest);

		var expectedDiagnosticResults = new DiagnosticResult[]
		{
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0002).WithSpan(13, 16, 13, 28).WithArguments("DatabaseName"),
		};

		await VerifyCS.VerifyCodeFixAsync(testContent, expectedDiagnosticResults, codeFixResult);
	}

	[TestMethod]
	public async Task MembersOrderedCorrectlyCodeFixer_works_with_method_names_to_order_first()
	{
		var testContent = await TestHelper.GetTestInputFileAsync(CancellationTokenForTest);
		var codeFixResult = await TestHelper.GetTestCodeFixResultFileAsync(CancellationTokenForTest);

		var expectedDiagnosticResults = new DiagnosticResult[]
		{
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0002).WithSpan(8, 16, 8, 43).WithArguments("InstanceMethodToPutAtTheTop"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0002).WithSpan(18, 21, 18, 46).WithArguments("StaticMethodToPutAtTheTop"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0002).WithSpan(21, 16, 21, 48).WithArguments("OtherInstanceMethodToPutAtTheTop"),
		};

		var editorConfigSettings = new List<(string, string)>()
		{
			("/.EditorConfig", $@"root = true

[*]
dotnet_code_quality.RBCS0002.method_names_to_order_first = StaticMethodToPutAtTheTop,OtherInstanceMethodToPutAtTheTop,InstanceMethodToPutAtTheTop
")
		};

		await VerifyCS.VerifyCodeFixAsync(testContent, expectedDiagnosticResults, codeFixResult, analyzerConfigFiles: editorConfigSettings);
	}
}
