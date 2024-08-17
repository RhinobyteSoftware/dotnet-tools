#nullable enable

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rhinobyte.CodeAnalysis.NetAnalyzers.Tests;

public static partial class CSharpCodeFixVerifier<TAnalyzer, TCodeFix>
	where TAnalyzer : DiagnosticAnalyzer, new()
	where TCodeFix : CodeFixProvider, new()
{
	/// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.Diagnostic()"/>
	public static DiagnosticResult Diagnostic()
		=> CSharpCodeFixVerifier<TAnalyzer, TCodeFix, DefaultVerifier>.Diagnostic();

	/// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.Diagnostic(string)"/>
	public static DiagnosticResult Diagnostic(string diagnosticId)
		=> CSharpCodeFixVerifier<TAnalyzer, TCodeFix, DefaultVerifier>.Diagnostic(diagnosticId);

	/// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.Diagnostic(DiagnosticDescriptor)"/>
	public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor)
		=> CSharpCodeFixVerifier<TAnalyzer, TCodeFix, DefaultVerifier>.Diagnostic(descriptor);

	/// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyAnalyzerAsync(string, DiagnosticResult[])"/>
	public static async Task VerifyAnalyzerAsync(
		string source,
		DiagnosticResult[]? expected = null,
		ICollection<string>? disabledDiagnostics = null,
		ICollection<(string filename, string fileContent)>? analyzerConfigFiles = null)
	{
		var test = new Test
		{
			TestCode = source,
		};

		if (disabledDiagnostics is not null)
		{
			test.DisabledDiagnostics.AddRange(disabledDiagnostics);
		}

		if (analyzerConfigFiles is not null)
		{
			foreach (var (filename, fileContent) in analyzerConfigFiles)
				test.TestState.AnalyzerConfigFiles.Add((filename, fileContent));
		}

		test.ExpectedDiagnostics.AddRange(expected ?? []);
		await test.RunAsync(CancellationToken.None);
	}

	/// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyCodeFixAsync(string, string)"/>
	public static async Task VerifyCodeFixAsync(string source, string fixedSource)
		=> await VerifyCodeFixAsync(source, DiagnosticResult.EmptyDiagnosticResults, fixedSource);

	/// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyCodeFixAsync(string, DiagnosticResult, string)"/>
	public static async Task VerifyCodeFixAsync(string source, DiagnosticResult expected, string fixedSource)
		=> await VerifyCodeFixAsync(source, [expected], fixedSource);

	/// <inheritdoc cref="CodeFixVerifier{TAnalyzer, TCodeFix, TTest, TVerifier}.VerifyCodeFixAsync(string, DiagnosticResult[], string)"/>
	public static async Task VerifyCodeFixAsync(
		string source,
		DiagnosticResult[] expected,
		string fixedSource,
		ICollection<string>? disabledDiagnostics = null,
		ICollection<(string filename, string fileContent)>? analyzerConfigFiles = null)
	{
		var test = new Test
		{
			TestCode = source,
			FixedCode = fixedSource,
		};

		if (disabledDiagnostics is not null)
		{
			test.DisabledDiagnostics.AddRange(disabledDiagnostics);
		}

		if (analyzerConfigFiles is not null)
		{
			foreach (var (filename, fileContent) in analyzerConfigFiles)
				test.TestState.AnalyzerConfigFiles.Add((filename, fileContent));
		}

		test.ExpectedDiagnostics.AddRange(expected);
		await test.RunAsync(CancellationToken.None);
	}
}
