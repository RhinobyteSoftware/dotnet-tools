using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace Rhinobyte.CodeAnalysis.NetAnalyzers;

/// <summary>
/// Code fix provider for the <see cref="UseExplicitNameForOptionalMethodParametersAnalyzer"/> diagnostics.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MembersOrderedCorrectlyAnalyzerCodeFixProvider)), Shared]
public class UseExplicitNameForOptionalMethodParametersCodeFixProvider : CodeFixProvider
{
	/// <inheritdoc/>
	public override ImmutableArray<string> FixableDiagnosticIds => [UseExplicitNameForOptionalMethodParametersAnalyzer.RBCS0010];

	internal static async Task<Document> ChangeOptionalParameterArgumentsToUseExplicitNameAsync(
		Document document,
		InvocationExpressionSyntax invocationExpressionSyntax,
		List<Diagnostic> optionalParameterDiagnostics,
		CancellationToken cancellationToken)
	{
		var oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (oldRoot is null)
			return document;

		var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
		if (semanticModel is null)
			return document;

		var methodSymbol = (IMethodSymbol?)semanticModel.GetSymbolInfo(invocationExpressionSyntax, cancellationToken).Symbol;
		if (methodSymbol is null)
			return document;

		var newArguments = invocationExpressionSyntax.ArgumentList.Arguments;
		foreach (var diagnostic in optionalParameterDiagnostics)
		{
			var argumentIndexToFix = diagnostic.Properties[UseExplicitNameForOptionalMethodParametersAnalyzer.ArgumentIndexPropertyName];
			var propertyName = diagnostic.Properties[UseExplicitNameForOptionalMethodParametersAnalyzer.ParameterNamePropertyName];
			if (string.IsNullOrEmpty(argumentIndexToFix) || string.IsNullOrEmpty(propertyName))
				continue;

			var argumentIndex = int.Parse(argumentIndexToFix);
			var oldArgument = newArguments[argumentIndex];

			var parameterNameSyntax = SyntaxFactory.IdentifierName(propertyName!);
			var nameColonSyntax = SyntaxFactory.NameColon(parameterNameSyntax);

			newArguments = newArguments.Replace(
				oldArgument,
				oldArgument.WithNameColon(nameColonSyntax)
			);
		}

		var newInvocationExpressionSyntax = invocationExpressionSyntax.WithArgumentList(
			invocationExpressionSyntax.ArgumentList.WithArguments(newArguments)
		);

		var newRoot = oldRoot.ReplaceNode(invocationExpressionSyntax, newInvocationExpressionSyntax);

#if DEBUG
		var newDocument = document.WithSyntaxRoot(newRoot);
		var newSyntaxTree = await newDocument.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
		var newSyntaxTreeContent = newSyntaxTree?.ToString();
		return newDocument;
#else
		return document.WithSyntaxRoot(newRoot);
#endif

	}

	/// <inheritdoc/>
	public sealed override FixAllProvider GetFixAllProvider() =>
		// See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
		WellKnownFixAllProviders.BatchFixer;

	/// <inheritdoc/>
	public override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		Dictionary<InvocationExpressionSyntax, List<Diagnostic>>? optionalParametersToMakeExplicit = null;
		foreach (var diagnostic in context.Diagnostics)
		{
			var diagnosticSpan = diagnostic.Location.SourceSpan;
			var node = root.FindNode(diagnosticSpan);

			while (node is not InvocationExpressionSyntax && node.Parent is not null)
				node = node.Parent;

			if (node is null || node is not InvocationExpressionSyntax invocationExpressionSyntax)
				continue;

			// Group the individual optional parameter diagnostics by the invocation expression they are associated with for a single code fix to the entire method invocation
			optionalParametersToMakeExplicit ??= new Dictionary<InvocationExpressionSyntax, List<Diagnostic>>();
			if (!optionalParametersToMakeExplicit.TryGetValue(invocationExpressionSyntax, out var diagnosticsForMethodInvocation))
			{
				diagnosticsForMethodInvocation = new List<Diagnostic>();
				optionalParametersToMakeExplicit.Add(invocationExpressionSyntax, diagnosticsForMethodInvocation);
			}

			diagnosticsForMethodInvocation.Add(diagnostic);
		}

		if (optionalParametersToMakeExplicit is null)
			return;

		foreach (var keyValuePair in optionalParametersToMakeExplicit)
		{
			var invocationExpression = keyValuePair.Key;
			var optionalParameterDiagnostics = keyValuePair.Value;

			var useExplicitNameForOptionalParametersCodeFixAction = CodeAction.Create(
				title: CodeFixResources.UseExplicitNameForOptionalParametersCodeFixTitle,
				createChangedDocument: (cancellationToken) => ChangeOptionalParameterArgumentsToUseExplicitNameAsync(
					context.Document,
					invocationExpression,
					optionalParameterDiagnostics,
					cancellationToken
				),
				equivalenceKey: CodeFixResources.UseExplicitNameForOptionalParametersCodeFixTitle
			);

			context.RegisterCodeFix(useExplicitNameForOptionalParametersCodeFixAction, optionalParameterDiagnostics);
		}
	}
}
