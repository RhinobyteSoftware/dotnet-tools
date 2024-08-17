using FluentAssertions;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.CodeAnalysis.NetAnalyzers.Utilities;
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
	public void ConvertStringToGroupOrderLookup_returns_the_expected_result()
	{
		MemberOrderingOptions.ConvertStringToGroupOrderLookup(null).Should().BeNull();
		MemberOrderingOptions.ConvertStringToGroupOrderLookup(string.Empty).Should().BeNull();

		// TODO: Add more use cases
	}

	[TestMethod]
	public async Task MembersOrderedCorrectlyAnalyzer_correctly_flags_symbols_not_alphabetical_by_group_partial_class()
	{
		var testContent = await TestHelper.GetTestInputFileAsync(CancellationTokenForTest);

		var expectedDiagnosticResults = new DiagnosticResult[]
		{
			// Partial file1
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0002).WithSpan(12, 22, 12, 33).WithArguments("ConstantOne"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0002).WithSpan(16, 23, 16, 30).WithArguments("_fieldB"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0002).WithSpan(31, 14, 31, 23).WithArguments("IsEnabled"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0002).WithSpan(38, 27, 38, 43).WithArguments("DoSomethingAsync"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0001).WithSpan(43, 16, 43, 34).WithArguments("OutOfOrderProperty"),

			// Partial file3
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0002).WithSpan(70, 22, 70, 35).WithArguments("ConstantThree"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0002).WithSpan(84, 16, 84, 25).WithArguments("AProperty"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0002).WithSpan(87, 16, 87, 23).WithArguments("AMethod"),
		};

		await VerifyCS.VerifyAnalyzerAsync(
			testContent,
			expected: expectedDiagnosticResults,
			disabledDiagnostics: [MembersOrderedCorrectlyAnalyzer.RBCS0005]
		);
	}

	[TestMethod]
	public async Task MembersOrderedCorrectlyAnalyzer_correctly_flags_symbols_that_arent_ordered_alphabetically_within_the_group()
	{
		var testContent = await TestHelper.GetTestInputFileAsync(CancellationTokenForTest);

		var expectedDiagnosticResults = new DiagnosticResult[]
		{
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0002).WithSpan(12, 23, 12, 34).WithArguments("ConstantOne"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0002).WithSpan(16, 24, 16, 31).WithArguments("_fieldB"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0002).WithSpan(31, 15, 31, 24).WithArguments("IsEnabled"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0002).WithSpan(38, 28, 38, 44).WithArguments("DoSomethingAsync"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0001).WithSpan(43, 17, 43, 35).WithArguments("OutOfOrderProperty"),
		};

		await VerifyCS.VerifyAnalyzerAsync(testContent, expectedDiagnosticResults);
		//await VerifyCS.VerifyAnalyzerAsync(testContent);
	}

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

	[TestMethod]
	public async Task MembersOrderedCorrectlyAnalyzer_enum_members_diagnostic()
	{
		var testContent = await TestHelper.GetTestInputFileAsync(CancellationTokenForTest);

		var expectedDiagnosticResults = new DiagnosticResult[]
		{
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0004).WithSpan(13, 2, 13, 8).WithArguments("ValueB"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.Rule_RBCS_0004).WithSpan(15, 2, 15, 8).WithArguments("ValueD"),
		};

		await VerifyCS.VerifyAnalyzerAsync(testContent, expectedDiagnosticResults);
	}

	[TestMethod]
	public async Task MembersOrderedCorrectlyAnalyzer_ignores_primary_constructor()
	{
		var testContent = await TestHelper.GetTestInputFileAsync(CancellationTokenForTest);

		var expectedDiagnosticResults = new DiagnosticResult[]
		{
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0002).WithSpan(13, 23, 13, 34).WithArguments("ConstantOne"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0002).WithSpan(17, 24, 17, 31).WithArguments("_fieldB"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0002).WithSpan(28, 15, 28, 24).WithArguments("IsEnabled"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0002).WithSpan(35, 28, 35, 44).WithArguments("DoSomethingAsync"),
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0001).WithSpan(40, 17, 40, 35).WithArguments("OutOfOrderProperty"),
		};

		await VerifyCS.VerifyAnalyzerAsync(testContent, expectedDiagnosticResults);
	}

	[TestMethod]
	public async Task MembersOrderedCorrectlyAnalyzer_ignores_primary_constructor2()
	{
		var testContent = await TestHelper.GetTestInputFileAsync(CancellationTokenForTest);

		await VerifyCS.VerifyAnalyzerAsync(testContent);
	}
}
