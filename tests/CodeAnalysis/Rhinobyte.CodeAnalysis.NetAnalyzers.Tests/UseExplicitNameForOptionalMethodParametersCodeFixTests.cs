using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System.Threading.Tasks;

using VerifyCSharpCodeFix = Rhinobyte.CodeAnalysis.NetAnalyzers.Tests.CSharpCodeFixVerifier<
	Rhinobyte.CodeAnalysis.NetAnalyzers.UseExplicitNameForOptionalMethodParametersAnalyzer,
	Rhinobyte.CodeAnalysis.NetAnalyzers.UseExplicitNameForOptionalMethodParametersCodeFixProvider>;

namespace Rhinobyte.CodeAnalysis.NetAnalyzers.Tests;

[TestClass]
public class UseExplicitNameForOptionalMethodParametersCodeFixTests
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
	public void TestMethod1(
		int requiredParam,
		string optionalParamOne = """",
		decimal optionalParamTwo = 2)
	{
		// No-op
	}

	public void TestMethod2(
		int param1,
		int param2,
		int param3)
	{
		// No-op
	}

	public void TestMethodCaller()
	{
		TestMethod1(1, ""Two"");

		TestMethod1(1, ""Two"", 3.0m);

		TestMethod1(1, ""Two"", optionalParamTwo: 3.0m);

		TestMethod2(1, 2, 3);
	}
}
";

		var expectedDiagnosticResults = new DiagnosticResult[]
		{
			// First method invocation (1 optional param)
			VerifyCSharpCodeFix.Diagnostic(UseExplicitNameForOptionalMethodParametersAnalyzer.Rule_RBCS_0010).WithSpan(22, 18, 22, 23).WithArguments("optionalParamOne"),

			// Second method invocation (2 optional params)
			VerifyCSharpCodeFix.Diagnostic(UseExplicitNameForOptionalMethodParametersAnalyzer.Rule_RBCS_0010).WithSpan(24, 18, 24, 23).WithArguments("optionalParamOne"),
			VerifyCSharpCodeFix.Diagnostic(UseExplicitNameForOptionalMethodParametersAnalyzer.Rule_RBCS_0010).WithSpan(24, 25, 24, 29).WithArguments("optionalParamTwo"),

			// 3rd method invocation (2 params, but only one needs fixed
			VerifyCSharpCodeFix.Diagnostic(UseExplicitNameForOptionalMethodParametersAnalyzer.Rule_RBCS_0010).WithSpan(26, 18, 26, 23).WithArguments("optionalParamOne"),
		};

		var expectedCodeAfterFix =
@"
public class TestClass
{
	public void TestMethod1(
		int requiredParam,
		string optionalParamOne = """",
		decimal optionalParamTwo = 2)
	{
		// No-op
	}

	public void TestMethod2(
		int param1,
		int param2,
		int param3)
	{
		// No-op
	}

	public void TestMethodCaller()
	{
		TestMethod1(1, optionalParamOne: ""Two"");

		TestMethod1(1, optionalParamOne: ""Two"", optionalParamTwo: 3.0m);

		TestMethod1(1, optionalParamOne: ""Two"", optionalParamTwo: 3.0m);

		TestMethod2(1, 2, 3);
	}
}
";


		await VerifyCSharpCodeFix.VerifyCodeFixAsync(testContent, expectedDiagnosticResults, expectedCodeAfterFix);
	}
}
