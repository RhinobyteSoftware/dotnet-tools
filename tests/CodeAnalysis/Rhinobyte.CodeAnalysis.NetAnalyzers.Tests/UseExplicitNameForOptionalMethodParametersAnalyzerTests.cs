using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;

using VerifyCSharp = Rhinobyte.CodeAnalysis.NetAnalyzers.Tests.CSharpAnalyzerVerifier<
	Rhinobyte.CodeAnalysis.NetAnalyzers.UseExplicitNameForOptionalMethodParametersAnalyzer>;

namespace Rhinobyte.CodeAnalysis.NetAnalyzers.Tests;

[TestClass]
public class UseExplicitNameForOptionalMethodParametersAnalyzerTests
{
	public CancellationToken CancellationTokenForTest => TestContext?.CancellationTokenSource?.Token ?? default;

	public TestContext TestContext { get; set; }

	[TestMethod]
	public async Task OptionalParameter_is_flagged_when_passed_positionally()
	{
		var testContent =
@"
public class TestClass
{
	public void TestMethod(
		int requiredParam,
		string optionalParamOne = """",
		decimal optionalParamTwo = 2)
	{
		// No-op
	}

	public void TestMethodCaller()
	{
		TestMethod(1, ""Two"");

		TestMethod(1, ""Two"", 3.0m);
	}
}
";

		var expectedDiagnosticResults = new DiagnosticResult[]
		{
			// First method invocation (1 optional param)
			VerifyCSharp.Diagnostic(UseExplicitNameForOptionalMethodParametersAnalyzer.Rule_RBCS_0010).WithSpan(14, 17, 14, 22).WithArguments("optionalParamOne"),

			// Second method invocation (2 optional params)
			VerifyCSharp.Diagnostic(UseExplicitNameForOptionalMethodParametersAnalyzer.Rule_RBCS_0010).WithSpan(16, 17, 16, 22).WithArguments("optionalParamOne"),
			VerifyCSharp.Diagnostic(UseExplicitNameForOptionalMethodParametersAnalyzer.Rule_RBCS_0010).WithSpan(16, 24, 16, 28).WithArguments("optionalParamTwo")
		};


		await VerifyCSharp.VerifyAnalyzerAsync(testContent, expectedDiagnosticResults);
	}

	[TestMethod]
	public async Task OptionalParameter_is_not_flagged_when_passed_explicitly_by_name1()
	{
		var testContent =
@"
public class TestClass
{
	public void TestMethod(
		int requiredParam,
		string optionalParamOne = """",
		decimal optionalParamTwo = 2)
	{
		// No-op
	}

	public void TestMethodCaller()
	{
		TestMethod(1, optionalParamOne: ""Two"");

		TestMethod(1, optionalParamTwo: 3.0m);

		TestMethod(1, optionalParamOne: ""Two"", optionalParamTwo: 3.0m);
	}
}
";

		await VerifyCSharp.VerifyAnalyzerAsync(testContent, []);
	}

	[TestMethod]
	public async Task OptionalParameter_is_not_flagged_when_passed_explicitly_by_name2()
	{
		var testContent =
@"
public class TestClass
{
	public void TestMethod(
		int requiredParam,
		string optionalParamOne = """",
		decimal optionalParamTwo = 2)
	{
		// No-op
	}

	public void TestMethodCaller()
	{
		TestMethod(1, ""Two"", optionalParamTwo: 3.0m);
	}
}
";

		var expectedDiagnosticResults = new DiagnosticResult[]
		{
			// Optional parameter one should be flagged but optional parameter two should not, since it's explicitly named
			VerifyCSharp.Diagnostic(UseExplicitNameForOptionalMethodParametersAnalyzer.Rule_RBCS_0010).WithSpan(14, 17, 14, 22).WithArguments("optionalParamOne"),
		};

		await VerifyCSharp.VerifyAnalyzerAsync(testContent, expectedDiagnosticResults);
	}
}
