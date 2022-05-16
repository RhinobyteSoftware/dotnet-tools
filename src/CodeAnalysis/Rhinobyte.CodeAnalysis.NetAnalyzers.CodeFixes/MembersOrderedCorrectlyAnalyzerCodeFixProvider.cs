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
		get { return ImmutableArray.Create(MembersOrderedCorrectlyAnalyzer.RBCS0001, MembersOrderedCorrectlyAnalyzer.RBCS0002); }
	}
#pragma warning restore IDE0025 // Use expression body for properties

	public static (int NewLineCount, int NonWhitespaceTriviaCount) CountTriviaTypes(SyntaxTriviaList triviaList)
	{
		var newlineCount = 0;
		var nonWhitespaceTriviaCount = 0;
		foreach (var trivia in triviaList)
		{
			if (trivia.IsKind(SyntaxKind.EndOfLineTrivia))
			{
				++newlineCount;
				continue;
			}

			if (!trivia.IsKind(SyntaxKind.WhitespaceTrivia))
			{
				++nonWhitespaceTriviaCount;
			}
		}

		return (newlineCount, nonWhitespaceTriviaCount);
	}

	/// <inheritdoc/>
	public sealed override FixAllProvider GetFixAllProvider()
	{
		// See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
		return WellKnownFixAllProviders.BatchFixer;
	}

	internal static bool IsNewlineNeeded(
		MemberGroupType? groupType,
		bool isStartOfGroup,
		int newlineCount,
		int nonWhitespaceTriviaCount)
	{
		if (newlineCount > 0)
			return false;

		if (isStartOfGroup || nonWhitespaceTriviaCount > 0)
			return true;

		return groupType == MemberGroupType.NestedEnumType
			|| groupType == MemberGroupType.NestedOtherType
			|| groupType == MemberGroupType.NestedRecordType
			|| groupType == MemberGroupType.Constructors
			|| groupType == MemberGroupType.StaticConstructors
			|| groupType == MemberGroupType.InstanceMethods
			|| groupType == MemberGroupType.StaticMethods;
	}

	internal static (int PreviousStartOfGroupIndex, int NewlineCount) LookForPreviousStartOfGroupValues(
		int currentGroupIndex,
		int currentItemIndex,
		SortedList<SortedMemberDeclaration, MemberDeclarationSyntax> sortedMembers)
	{
		var previousStartOfNewGroupIndex = -1;
		var newlineCount = -1;
		for (var nextItemIndex = currentItemIndex + 1; nextItemIndex < sortedMembers.Count; ++nextItemIndex)
		{
			var nextSortedPair = sortedMembers.ElementAt(nextItemIndex);
			if (nextSortedPair.Key.GroupIndex == SortedMemberDeclaration.ImplicitGroupIndex)
				continue;

			if (nextSortedPair.Key.GroupIndex != currentGroupIndex)
				return (previousStartOfNewGroupIndex, newlineCount);

			var nextLeadingTrivia = nextSortedPair.Value.GetLeadingTrivia();
			var (nextNelineCount, nextNonWhitespaceCount) = CountTriviaTypes(nextLeadingTrivia);

			if (nextNelineCount < 1 || nextNonWhitespaceCount > 0)
				continue;

			if (previousStartOfNewGroupIndex != -1)
			{
				// If more than one item in the same group has newlines then don't treat it like a group that does not have newlines between items
				return (-1, -1);
			}

			previousStartOfNewGroupIndex = nextItemIndex;
			newlineCount = nextNelineCount;
		}

		return (previousStartOfNewGroupIndex, newlineCount);
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
				sortedMembers.Add(new SortedMemberDeclaration(null, memberCount++), memberSyntax);
				continue;
			}

			var groupType = MembersOrderedCorrectlyAnalyzer.GetGroupType(memberSymbol);
			if (!memberSymbol.CanBeReferencedByName && groupType != MemberGroupType.Constructors && groupType != MemberGroupType.StaticConstructors)
			{
				sortedMembers.Add(new SortedMemberDeclaration(groupType, memberCount++), memberSyntax);
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
				? new SortedMemberDeclaration(matchedGroupIndex, groupType, memberCount++, memberSymbol.Name)
				: new SortedMemberDeclaration(groupType, memberCount++);

			sortedMembers.Add(sortKey, memberSyntax);
		}

		var reorderedMembers = new List<MemberDeclarationSyntax>();
		var currentGroupIndex = -1;
		var currentItemIndex = -1;
		var lastMemberIndex = sortedMembers.Count - 1;
		var removeNewlineItemIndex = -1;

		foreach (var sortedPair in sortedMembers)
		{
			++currentItemIndex;

			if (sortedPair.Key.GroupIndex == SortedMemberDeclaration.ImplicitGroupIndex)
			{
				reorderedMembers.Add(sortedPair.Value);
				continue;
			}

			var isStartOfNewGroup = (currentGroupIndex > -1) && sortedPair.Key.GroupIndex != currentGroupIndex;
			currentGroupIndex = sortedPair.Key.GroupIndex;

			var leadingTrivia = sortedPair.Value.GetLeadingTrivia();
			if (currentItemIndex == removeNewlineItemIndex)
			{
				var newlineTrivia = leadingTrivia.Where(trivia => trivia.IsKind(SyntaxKind.EndOfLineTrivia)).ToArray();
				foreach (var triviaToRemove in newlineTrivia)
					leadingTrivia = leadingTrivia.Remove(triviaToRemove);

				var syntaxWithNewlineRemoved = sortedPair.Value.WithLeadingTrivia(leadingTrivia);
				reorderedMembers.Add(syntaxWithNewlineRemoved);
				continue;
			}

			var (newlineCount, nonWhitespaceTriviaCount) = CountTriviaTypes(leadingTrivia);
			var needsNewlineAdded = IsNewlineNeeded(sortedPair.Key.GroupType, isStartOfNewGroup, newlineCount, nonWhitespaceTriviaCount);

			if (!needsNewlineAdded)
			{
				reorderedMembers.Add(sortedPair.Value);
				continue;
			}

			var newlinesToAdd = 1;
			if (isStartOfNewGroup)
			{
				var (previousStartOfGroupIndex, previousStartOfGroupNewlines) = LookForPreviousStartOfGroupValues(currentGroupIndex, currentItemIndex, sortedMembers);
				if (previousStartOfGroupIndex > -1)
					removeNewlineItemIndex = previousStartOfGroupIndex;

				if (previousStartOfGroupNewlines > 0)
					newlinesToAdd = previousStartOfGroupNewlines;
			}

			// Prepend the necessary number of newlines
			for (var newlineIndex = 0; newlineIndex < newlinesToAdd; ++newlineIndex)
				leadingTrivia = leadingTrivia.Insert(0, SyntaxFactory.ElasticCarriageReturnLineFeed);

			var syntaxWithNewlineAdded = sortedPair.Value.WithLeadingTrivia(leadingTrivia);
			reorderedMembers.Add(syntaxWithNewlineAdded);
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

		public SortedMemberDeclaration(
			MemberGroupType? groupType,
			int implicitMemberIndex)
		{
			GroupIndex = ImplicitGroupIndex;
			GroupType = groupType;
			ImplicitMemberIndex = implicitMemberIndex;
			Name = null;
		}

		public SortedMemberDeclaration(
			int groupIndex,
			MemberGroupType groupType,
			int memberIndex,
			string name)
		{
			GroupIndex = groupIndex;
			GroupType = groupType;
			ImplicitMemberIndex = memberIndex;
			Name = name;
		}

		public int GroupIndex { get; }

		public MemberGroupType? GroupType { get; }

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
