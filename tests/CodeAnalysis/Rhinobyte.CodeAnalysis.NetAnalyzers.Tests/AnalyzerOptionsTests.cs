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
public class AnalyzerOptionsTests
{
	public CancellationToken CancellationTokenForTest => TestContext?.CancellationTokenSource?.Token ?? default;

	public TestContext TestContext { get; set; }

	//Diagnostic and CodeFix both triggered and checked for
	[TestMethod]
	public async Task MembersOrderedCorrectly_orders_custom_property_names_first_when_configured_via_editorconfig_file1()
	{
		const string subdirectoryName = nameof(AnalyzerOptionsTests);
		const string testCaseFilename = "PropertyNamesToOrderFirstTestCase1";
		var testContent = await TestHelper.GetTestInputFileAsync(CancellationTokenForTest, subdirectoryName, testCaseFilename);
		var codeFixResult = await TestHelper.GetTestCodeFixResultFileAsync(CancellationTokenForTest, subdirectoryName, testCaseFilename);

		var expectedDiagnosticResults = new DiagnosticResult[]
		{
			// /0/Test0.cs(12,16): info RBCS0002: Member 'Bravo' is not ordered alphabetically
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0002).WithSpan(12, 16, 12, 21).WithArguments("Bravo"),
			// /0/Test0.cs(14,16): info RBCS0002: Member 'Charlie' is not ordered alphabetically
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0002).WithSpan(14, 16, 14, 23).WithArguments("Charlie"),
		};

		// TODO: Figure out why their test runner is also adding spaces, only to the lines where I adjust the whitespace, on the output when applying the fixes

		var editorConfigSettings = new List<(string, string)>()
		{
			("/.editorconfig", $@"root = true

[*]
dotnet_code_quality.RBCS0001.type_members_group_order = Constants,StaticReadonlyFields:StaticMutableFields:StaticProperties:StaticConstructors:MutableInstanceFields,ReadonlyInstanceFields:InstanceProperties:Constructors:StaticMethods,InstanceMethods:NestedEnumType,NestedRecordType,NestedOtherType
dotnet_code_quality.RBCS0002.property_names_to_order_first = Id
")
		};

		await VerifyCS.VerifyCodeFixAsync(testContent, expectedDiagnosticResults, codeFixResult, editorConfigSettings);
		//await VerifyCS.VerifyCodeFixAsync(testContent, codeFixResult);
	}

	//Diagnostic and CodeFix both triggered and checked for
	[TestMethod]
	public async Task MembersOrderedCorrectly_orders_custom_property_names_first_when_configured_via_editorconfig_file2()
	{
		const string subdirectoryName = nameof(AnalyzerOptionsTests);
		const string testCaseFilename = "PropertyNamesToOrderFirstTestCase2";
		var testContent = await TestHelper.GetTestInputFileAsync(CancellationTokenForTest, subdirectoryName, testCaseFilename);
		var codeFixResult = await TestHelper.GetTestCodeFixResultFileAsync(CancellationTokenForTest, subdirectoryName, testCaseFilename);

		//var expectedDiagnosticResults = System.Array.Empty<DiagnosticResult>();

		var expectedDiagnosticResults = new DiagnosticResult[]
		{
			// /0/Test0.cs(12,16): info RBCS0002: Member 'Id' is not ordered alphabetically
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0002).WithSpan(12, 16, 12, 18).WithArguments("Id"),
			// /0/Test0.cs(14,16): info RBCS0002: Member 'Charlie' is not ordered alphabetically
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0002).WithSpan(14, 16, 14, 23).WithArguments("Charlie"),
		};


		// TODO: Figure out why their test runner is also adding spaces, only to the lines where I adjust the whitespace, on the output when applying the fixes

		var editorConfigSettings = new List<(string, string)>()
		{
			("/.editorconfig", $@"root = true

[*]
dotnet_code_quality.RBCS0001.type_members_group_order = Constants,StaticReadonlyFields:StaticMutableFields:StaticProperties:StaticConstructors:MutableInstanceFields,ReadonlyInstanceFields:InstanceProperties:Constructors:StaticMethods,InstanceMethods:NestedEnumType,NestedRecordType,NestedOtherType
dotnet_code_quality.RBCS0002.property_names_to_order_first = Id
")
		};

		await VerifyCS.VerifyCodeFixAsync(testContent, expectedDiagnosticResults, codeFixResult, editorConfigSettings);
	}


	//Diagnostic and CodeFix both triggered and checked for
	[TestMethod]
	public async Task MembersOrderedCorrectly_uses_the_custom_group_order_from_the_editorconfig()
	{
		const string subdirectoryName = nameof(AnalyzerOptionsTests);
		const string testCaseFilename = "EditorConfigGroupingTestCase1";
		var testContent = await TestHelper.GetTestInputFileAsync(CancellationTokenForTest, subdirectoryName, testCaseFilename);
		var codeFixResult = await TestHelper.GetTestCodeFixResultFileAsync(CancellationTokenForTest, subdirectoryName, testCaseFilename);

		var expectedDiagnosticResults = new DiagnosticResult[]
		{
			// /0/Test0.cs(33,22): info RBCS0002: Member 'ConstantD' is not ordered alphabetically
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0002).WithSpan(33, 22, 33, 31).WithArguments("ConstantD"),
			// /0/Test0.cs(35,22): info RBCS0002: Member 'ConstantB' is not ordered alphabetically
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0002).WithSpan(35, 22, 35, 31).WithArguments("ConstantB"),
			// /0/Test0.cs(39,21): info RBCS0002: Member 'StaticPropertyD' is not ordered alphabetically
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0002).WithSpan(39, 21, 39, 36).WithArguments("StaticPropertyD"),
			// /0/Test0.cs(41,21): info RBCS0002: Member 'StaticPropertyB' is not ordered alphabetically
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0002).WithSpan(41, 21, 41, 36).WithArguments("StaticPropertyB"),
			// /0/Test0.cs(48,13): info RBCS0001: Member 'PropertyC' is not ordered correctly by group
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0001).WithSpan(48, 13, 48, 22).WithArguments("PropertyC"),
			// /0/Test0.cs(49,13): info RBCS0001: Member 'PropertyE' is not ordered correctly by group
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0001).WithSpan(49, 13, 49, 22).WithArguments("PropertyE"),
			// /0/Test0.cs(50,13): info RBCS0001: Member 'PropertyD' is not ordered correctly by group
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0001).WithSpan(50, 13, 50, 22).WithArguments("PropertyD"),
			// /0/Test0.cs(51,13): info RBCS0001: Member 'PropertyZ' is not ordered correctly by group
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0001).WithSpan(51, 13, 51, 22).WithArguments("PropertyZ"),
			// /0/Test0.cs(52,13): info RBCS0001: Member 'PropertyB' is not ordered correctly by group
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0001).WithSpan(52, 13, 52, 22).WithArguments("PropertyB"),
			// /0/Test0.cs(58,27): info RBCS0002: Member 'MethodD' is not ordered alphabetically
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0002).WithSpan(58, 27, 58, 34).WithArguments("MethodD"),
			// /0/Test0.cs(65,23): info RBCS0002: Member 'MethodB' is not ordered alphabetically
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RuleRBCS0002).WithSpan(65, 23, 65, 30).WithArguments("MethodB"),
		};

		// TODO: Figure out why their test runner is also adding spaces, only to the lines where I adjust the whitespace, on the output when applying the fixes

		var editorConfigSettings = new List<(string, string)>()
		{
			("/.editorconfig", $@"root = true

[*]
dotnet_code_quality.RBCS0001.type_members_group_order = Constants,StaticReadonlyFields:StaticMutableFields:StaticProperties:StaticConstructors:MutableInstanceFields,ReadonlyInstanceFields:InstanceProperties:Constructors:StaticMethods,InstanceMethods:NestedEnumType,NestedRecordType,NestedOtherType
")
		};

		await VerifyCS.VerifyCodeFixAsync(testContent, expectedDiagnosticResults, codeFixResult, editorConfigSettings);
		//await VerifyCS.VerifyCodeFixAsync(testContent, codeFixResult);
	}

}
