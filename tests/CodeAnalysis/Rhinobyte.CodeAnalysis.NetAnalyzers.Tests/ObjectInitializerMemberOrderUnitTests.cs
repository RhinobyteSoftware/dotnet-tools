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
public class ObjectInitializerMemberOrderUnitTests
{
	public CancellationToken CancellationTokenForTest => TestContext?.CancellationTokenSource?.Token ?? default;

	public TestContext TestContext { get; set; }


	[TestMethod]
	public async Task MembersOrderedCorrectlyAnalyzer_correctly_flags_object_initializer_statements_where_the_member_names_are_not_ordered_correctly1()
	{
		const string subdirectoryName = nameof(ObjectInitializerMemberOrderUnitTests);
		const string testCaseFilename = "AnalyzerFlagsObjectInitializerTestCase1";
		var testContent = await TestHelper.GetTestInputFileAsync(CancellationTokenForTest, subdirectoryName, testCaseFilename);
		var codeFixResult = await TestHelper.GetTestCodeFixResultFileAsync(CancellationTokenForTest, subdirectoryName, testCaseFilename);

		var expectedDiagnosticResults = new DiagnosticResult[]
		{
			// /0/Test0.cs(16,3): info RBCS0003: The member assignments 'Charlie, Golf, Id, Alpha' are out of order
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0003).WithSpan(16, 3, 23, 4).WithArguments("Charlie, Golf, Id, Alpha"),
			// /0/Test0.cs(41,3): info RBCS0003: The member assignments 'Charlie, Golf, Id, Alpha' are out of order
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0003).WithSpan(41, 3, 48, 4).WithArguments("Charlie, Golf, Id, Alpha"),
		};

		await VerifyCS.VerifyCodeFixAsync(testContent, expectedDiagnosticResults, codeFixResult);
	}


	[TestMethod]
	public async Task MembersOrderedCorrectlyAnalyzer_correctly_flags_object_initializer_statements_where_the_member_names_are_not_ordered_correctly2()
	{
		const string subdirectoryName = nameof(ObjectInitializerMemberOrderUnitTests);
		const string testCaseFilename = "AnalyzerFlagsObjectInitializerTestCase2";
		var testContent = await TestHelper.GetTestInputFileAsync(CancellationTokenForTest, subdirectoryName, testCaseFilename);
		var codeFixResult = await TestHelper.GetTestCodeFixResultFileAsync(CancellationTokenForTest, subdirectoryName, testCaseFilename);

		var expectedDiagnosticResults = new DiagnosticResult[]
		{
			// /0/Test0.cs(16,3): info RBCS0003: The member assignments 'Charlie, Golf, Alpha' are out of order
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0003).WithSpan(16, 3, 23, 4).WithArguments("Charlie, Golf, Alpha"),
			// /0/Test0.cs(26,3): info RBCS0003: The member assignments 'Id' are out of order
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0003).WithSpan(26, 3, 33, 4).WithArguments("Id"),
			// /0/Test0.cs(41,3): info RBCS0003: The member assignments 'Charlie, Golf, Id, Alpha' are out of order
			VerifyCS.Diagnostic(MembersOrderedCorrectlyAnalyzer.RBCS0003).WithSpan(41, 3, 48, 4).WithArguments("Charlie, Golf, Id, Alpha"),
		};

		var editorConfigSettings = new List<(string, string)>()
		{
			("/.EditorConfig", $@"root = true

[*]
dotnet_code_quality.RBCS0001.type_members_group_order = Constants,StaticReadonlyFields:StaticMutableFields:StaticProperties:StaticConstructors:MutableInstanceFields,ReadonlyInstanceFields:InstanceProperties:Constructors:StaticMethods,InstanceMethods:NestedEnumType,NestedRecordType,NestedOtherType
dotnet_code_quality.RBCS0002.property_names_to_order_first = Id
")
		};

		await VerifyCS.VerifyCodeFixAsync(testContent, expectedDiagnosticResults, codeFixResult, editorConfigSettings);
	}
}
