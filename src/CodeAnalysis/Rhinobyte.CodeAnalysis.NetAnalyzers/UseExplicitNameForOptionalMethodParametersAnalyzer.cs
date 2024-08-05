using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Rhinobyte.CodeAnalysis.NetAnalyzers.Utilities;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Rhinobyte.CodeAnalysis.NetAnalyzers;

/// <summary>
/// Analyzer for detecting when method invocations pass optional parameters using positional arguments only and should instead pass them as explicitly named parameters.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class UseExplicitNameForOptionalMethodParametersAnalyzer : DiagnosticAnalyzer
{
	internal const string ArgumentIndexPropertyName = "ArgumentIndex";
	internal const string ParameterNamePropertyName = "ParameterName";

	/// <summary>
	/// The diagnostic rule id for optional method parameters being passed using positional arguments that should be changed to using explicit parameter names.
	/// </summary>
	public const string RBCS0010 = nameof(RBCS0010);

	internal static readonly DiagnosticDescriptor Rule_RBCS_0010 = DiagnosticDescriptorHelper.Create(
		RBCS0010,
		DiagnosticDescriptorHelper.MaintainabilityCategory,
		new LocalizableResourceString(nameof(AnalyzerResources.RBCS_0010_AnalyzerDescription), AnalyzerResources.ResourceManager, typeof(AnalyzerResources)),
		DiagnosticSeverity.Info,
		new LocalizableResourceString(nameof(AnalyzerResources.RBCS_0010_AnalyzerMessageFormat), AnalyzerResources.ResourceManager, typeof(AnalyzerResources)),
		new LocalizableResourceString(nameof(AnalyzerResources.RBCS_0010_AnalyzerTitle), AnalyzerResources.ResourceManager, typeof(AnalyzerResources)),
		isEnabledByDefault: true
	);

	/// <inheritdoc/>
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule_RBCS_0010];

	internal static void AnalyzeMethodInvocationSyntaxNodes(SyntaxNodeAnalysisContext analysisContext)
	{
		if (analysisContext.Node is not InvocationExpressionSyntax invocationSyntax)
		{
			return;
		}

		if (invocationSyntax.IsMissing)
		{
			// Ignore nodes that the parser generated as being an expected node that was missing from the source code
			return;
		}

		var semanticModel = analysisContext.SemanticModel;
		var methodSymbol = (IMethodSymbol?)semanticModel.GetSymbolInfo(invocationSyntax, analysisContext.CancellationToken).Symbol;
		if (methodSymbol is null)
		{
			return;
		}

		var argumentIndex = -1;
		foreach (var argumentSyntax in invocationSyntax.ArgumentList.Arguments)
		{
			++argumentIndex;

			if (argumentSyntax.NameColon is not null)
			{
				// Argument is already explicitly named
				continue;
			}

			var parameterSymbol = methodSymbol.Parameters.ElementAtOrDefault(argumentIndex);
			if (parameterSymbol?.IsOptional != true)
			{
				continue;
			}

			var diagnosticProperties = ImmutableDictionary.CreateRange(
				new KeyValuePair<string, string?>[]
				{
					new KeyValuePair<string, string?>( ArgumentIndexPropertyName, argumentIndex.ToString()),
					new KeyValuePair<string, string?>( ParameterNamePropertyName, parameterSymbol.Name)
				});

			analysisContext.ReportDiagnostic(Diagnostic.Create(Rule_RBCS_0010, argumentSyntax.GetLocation(), properties: diagnosticProperties, messageArgs: parameterSymbol.Name));
		}
	}

	/// <inheritdoc/>
	public override void Initialize(AnalysisContext analysisContext)
	{
		analysisContext.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		analysisContext.EnableConcurrentExecution();

		analysisContext.RegisterSyntaxNodeAction(AnalyzeMethodInvocationSyntaxNodes, SyntaxKind.InvocationExpression);
	}
}
