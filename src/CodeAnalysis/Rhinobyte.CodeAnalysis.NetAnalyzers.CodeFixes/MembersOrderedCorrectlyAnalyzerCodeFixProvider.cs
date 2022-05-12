using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rhinobyte.CodeAnalysis.NetAnalyzers;

/// <summary>
/// Code fix provider for consistent ordering of members within a type
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MembersOrderedCorrectlyAnalyzerCodeFixProvider)), Shared]
public class MembersOrderedCorrectlyAnalyzerCodeFixProvider : CodeFixProvider
{
#pragma warning disable IDE0025 // Use expression body for properties
	/// <inheritdoc/>
	public sealed override ImmutableArray<string> FixableDiagnosticIds
	{
		get { return ImmutableArray.Create(MembersOrderedCorrectlyAnalyzer.RBCS0001); }
	}
#pragma warning restore IDE0025 // Use expression body for properties

	/// <inheritdoc/>
	public sealed override FixAllProvider GetFixAllProvider()
	{
		// See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
		return WellKnownFixAllProviders.BatchFixer;
	}

	/// <inheritdoc/>
	public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
	{
		var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
		if (root is null)
			return;

		var reorderCodeFixes = new Dictionary<TypeDeclarationSyntax, CodeAction>();
		foreach (var diagnosticToFix in context.Diagnostics)
		{
			var diagnosticSpan = diagnosticToFix.Location.SourceSpan;

			var typeDeclarationSyntaxNode = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();
			if (typeDeclarationSyntaxNode is null)
				continue;

			if (!reorderCodeFixes.TryGetValue(typeDeclarationSyntaxNode, out var codeFixAction))
			{
				codeFixAction = CodeAction.Create(
					title: CodeFixResources.MemberOrderCodeFixTitle,
					createChangedDocument: (cancellationToken) => ReorderTypeMembersAsync(context.Document, typeDeclarationSyntaxNode, cancellationToken),
					equivalenceKey: nameof(CodeFixResources.MemberOrderCodeFixTitle)
				);
				reorderCodeFixes[typeDeclarationSyntaxNode] = codeFixAction;
			}

			// Register a code action that will invoke the fix.
			context.RegisterCodeFix(codeFixAction, diagnosticToFix);
		}
	}

	private static async Task<Document> ReorderTypeMembersAsync(Document document, TypeDeclarationSyntax typeDeclaration, CancellationToken cancellationToken)
	{
		var oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (oldRoot is null)
			return document;

		// Compute new member order
		var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
		if (semanticModel is null)
			return document;

		var typeDeclarationSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration, cancellationToken);
		if (typeDeclarationSymbol is null)
			return document;

		var memberSymbols = typeDeclarationSymbol.GetMembers();

		// TODO: Define and load code style configuration values somehow
		var groupOrder = MembersOrderedCorrectlyAnalyzer.DefaultGroupOrder;
		var members = typeDeclaration.Members;

		var sortedMembers = new SortedList<SortedMemberDeclaration, MemberDeclarationSyntax>();
		var memberCount = 0;
		foreach (var memberSyntax in members)
		{
			var memberSymbol = memberSymbols.FirstOrDefault(symbol =>
				!symbol.IsImplicitlyDeclared
				&& symbol.Locations.Any(symbolLocation => memberSyntax.Span.Contains(symbolLocation.SourceSpan)));

			if (memberSymbol is null)
			{
				sortedMembers.Add(new SortedMemberDeclaration(memberCount++), memberSyntax);
				continue;
			}

			var groupType = MembersOrderedCorrectlyAnalyzer.GetGroupType(memberSymbol);
			if (!memberSymbol.CanBeReferencedByName && groupType != MemberGroupType.Constructors && groupType != MemberGroupType.StaticConstructors)
			{
				sortedMembers.Add(new SortedMemberDeclaration(memberCount++), memberSyntax);
				continue;
			}

			var matchedGroupIndex = -1;
			for (var groupIndex = 0; groupIndex < groupOrder.Length; ++groupIndex)
			{
				if (groupOrder[groupIndex].Contains(groupType))
				{
					matchedGroupIndex = groupIndex;
					break;
				}
			}

			Debug.Assert(matchedGroupIndex > -1);
			var sortKey = matchedGroupIndex > -1
				? new SortedMemberDeclaration(matchedGroupIndex, memberCount++, memberSymbol.Name)
				: new SortedMemberDeclaration(memberCount++);

			sortedMembers.Add(sortKey, memberSyntax);
		}

		var reorderedMembers = new List<MemberDeclarationSyntax>();
		foreach (var sortedPair in sortedMembers)
		{
			reorderedMembers.Add(sortedPair.Value);
		}

		var reorderedSyntaxList = new SyntaxList<MemberDeclarationSyntax>(reorderedMembers);
		var newTypeDeclaration = typeDeclaration.WithMembers(reorderedSyntaxList);

		var newRoot = oldRoot.ReplaceNode(typeDeclaration, newTypeDeclaration);

#if DEBUG
		var newDocument = document.WithSyntaxRoot(newRoot);
		var newSyntaxTree = await newDocument.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
		var newSyntaxTreeContent = newSyntaxTree?.ToString();
		return newDocument;
#else
		return document.WithSyntaxRoot(newRoot);
#endif
	}

	internal struct SortedMemberDeclaration : IComparable<SortedMemberDeclaration>
	{
		internal const int ImplicitGroupIndex = -1;

		public SortedMemberDeclaration(int implicitMemberIndex)
		{
			GroupIndex = ImplicitGroupIndex;
			ImplicitMemberIndex = implicitMemberIndex;
			Name = null;
		}

		public SortedMemberDeclaration(int groupIndex, int memberIndex, string name)
		{
			GroupIndex = groupIndex;
			ImplicitMemberIndex = memberIndex;
			Name = name;
		}

		public int GroupIndex { get; }

		public int ImplicitMemberIndex { get; }

		public string? Name { get; }

		public static int Compare(SortedMemberDeclaration x, SortedMemberDeclaration y)
		{
			var comparison = x.GroupIndex.CompareTo(y.GroupIndex);

			if (comparison == 0)
				comparison = string.CompareOrdinal(x.Name, y.Name);

			if (comparison == 0)
				comparison = x.ImplicitMemberIndex.CompareTo(y.ImplicitMemberIndex);

			return comparison;
		}

		public int CompareTo(SortedMemberDeclaration other) => Compare(this, other);
	}
}
