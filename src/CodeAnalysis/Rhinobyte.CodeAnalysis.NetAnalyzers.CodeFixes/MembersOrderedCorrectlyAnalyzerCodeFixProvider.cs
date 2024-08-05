using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Rhinobyte.CodeAnalysis.NetAnalyzers.Utilities;
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
	/// <inheritdoc/>
	public sealed override ImmutableArray<string> FixableDiagnosticIds
		=> [MembersOrderedCorrectlyAnalyzer.RBCS0001, MembersOrderedCorrectlyAnalyzer.RBCS0002, MembersOrderedCorrectlyAnalyzer.RBCS0003];

	/// <summary>
	/// Count the number of newlines and non-whitespace trivia in the provided trivia list.
	/// </summary>
	/// <remarks>
	/// Our code fix provider uses this to determine if newlines need to be added or removed when automatically re-ordering members.
	/// </remarks>
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
	public sealed override FixAllProvider GetFixAllProvider() =>
		// See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
		WellKnownFixAllProviders.BatchFixer;

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
		SortedList<SortedMemberKey, MemberDeclarationSyntax> sortedMembers)
	{
		var previousStartOfNewGroupIndex = -1;
		var newlineCount = -1;
		for (var nextItemIndex = currentItemIndex + 1; nextItemIndex < sortedMembers.Count; ++nextItemIndex)
		{
			var nextSortedPair = sortedMembers.ElementAt(nextItemIndex);
			if (nextSortedPair.Key.GroupIndex == SortedMemberKey.ImplicitGroupIndex)
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

		List<Diagnostic>? typeMemberDiagnosticsToFix = null;
		List<TypeDeclarationSyntax>? typeDeclarationsToFix = null;

		foreach (var diagnosticToFix in context.Diagnostics)
		{
			var diagnosticSpan = diagnosticToFix.Location.SourceSpan;

			if (diagnosticToFix.Id == MembersOrderedCorrectlyAnalyzer.RBCS0003)
			{
				var initializerExpressionSyntax = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<InitializerExpressionSyntax>().First();
				if (initializerExpressionSyntax is null)
					continue;

				_ = diagnosticToFix.Properties.TryGetValue(nameof(MemberOrderingOptions.ArePropertyNamesToOrderFirstCaseSensitive), out var arePropertyNamesToOrderFirstCaseSensitiveRaw);
				_ = bool.TryParse(arePropertyNamesToOrderFirstCaseSensitiveRaw, out var propertyNamesToOrderFirstAreCaseSensitive);

				_ = diagnosticToFix.Properties.TryGetValue(nameof(MemberOrderingOptions.GroupOrderSettings), out var groupOrderLookupString);
				_ = diagnosticToFix.Properties.TryGetValue(nameof(MemberOrderingOptions.PropertyNamesToOrderFirst), out var propertyNamesToOrderFirst);

				var objectInitializerCodeFixAction = CodeAction.Create(
					title: CodeFixResources.MemberAssignmentOrderCodeFixTitle,
					createChangedDocument: (cancellationToken) => ReorderObjectInitializerMemberAssignmentsAsync(
						context.Document,
						groupOrderLookupString,
						initializerExpressionSyntax,
						propertyNamesToOrderFirst,
						propertyNamesToOrderFirstAreCaseSensitive,
						cancellationToken
					),
					equivalenceKey: nameof(CodeFixResources.MemberAssignmentOrderCodeFixTitle)
				);

				// Register the object initializer fixes individually so they can be applied individually, if desired
				context.RegisterCodeFix(objectInitializerCodeFixAction, diagnosticToFix);

				continue;
			}

			var typeDeclarationSyntaxNode = root.FindToken(diagnosticSpan.Start).Parent?.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();
			if (typeDeclarationSyntaxNode is null)
				continue;

			typeMemberDiagnosticsToFix ??= [];
			typeMemberDiagnosticsToFix.Add(diagnosticToFix);
			typeDeclarationsToFix ??= [];
			typeDeclarationsToFix.Add(typeDeclarationSyntaxNode);
		}

		if (typeMemberDiagnosticsToFix is not null && typeDeclarationsToFix is not null)
		{
			var firstDiagnosticToFix = typeMemberDiagnosticsToFix.First();

			_ = firstDiagnosticToFix.Properties.TryGetValue(nameof(MemberOrderingOptions.ArePropertyNamesToOrderFirstCaseSensitive), out var arePropertyNamesToOrderFirstCaseSensitiveRaw);
			_ = bool.TryParse(arePropertyNamesToOrderFirstCaseSensitiveRaw, out var propertyNamesToOrderFirstAreCaseSensitive);

			_ = firstDiagnosticToFix.Properties.TryGetValue(nameof(MemberOrderingOptions.GroupOrderSettings), out var groupOrderLookupString);
			_ = firstDiagnosticToFix.Properties.TryGetValue(nameof(MemberOrderingOptions.PropertyNamesToOrderFirst), out var propertyNamesToOrderFirst);

			var codeFixAction = CodeAction.Create(
				title: CodeFixResources.MemberOrderCodeFixTitle,
				createChangedDocument: (cancellationToken) => ReorderTypeMembersAsync(
					context.Document,
					groupOrderLookupString,
					propertyNamesToOrderFirst,
					propertyNamesToOrderFirstAreCaseSensitive,
					typeDeclarationsToFix,
					cancellationToken
				),
				equivalenceKey: nameof(CodeFixResources.MemberOrderCodeFixTitle)
			);

			// Only need to register one document fix for the type member re-order, so pass it all the individual member diagnostics that are being fixed
			context.RegisterCodeFix(codeFixAction, typeMemberDiagnosticsToFix);
		}
	}

	private static async Task<Document> ReorderObjectInitializerMemberAssignmentsAsync(
		Document document,
		string? groupOrderLookupString,
		InitializerExpressionSyntax initializerExpressionSyntax,
		string? propertyNamesToOrderFirstRawValue,
		bool propertyNamesToOrderFirstAreCaseSensitive,
		CancellationToken cancellationToken)
	{
		var oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (oldRoot is null)
			return document;

		var childExpressions = initializerExpressionSyntax.Expressions;
		var childSeparatorCount = childExpressions.SeparatorCount;

		var propertyNamesToOrderFirst = MemberOrderingOptions.GetPropertyNamesToOrderFirst(propertyNamesToOrderFirstRawValue);
		var propertyNameOrderComparison = propertyNamesToOrderFirstAreCaseSensitive
			? StringComparison.Ordinal
			: StringComparison.OrdinalIgnoreCase;

		var isAlphabetizing = propertyNamesToOrderFirst is null;
		var memberCount = 0;
		var sortedChildExpressions = new SortedList<SortedMemberKey, (ExpressionSyntax, SyntaxToken?)>();

		var childIndex = -1;
		foreach (var child in childExpressions)
		{
			++childIndex;
			var childSeparatorToken = childIndex < childSeparatorCount
				? childExpressions.GetSeparator(childIndex)
				: (SyntaxToken?)null;

			var assignmentExpressionSyntax = child as AssignmentExpressionSyntax;
			var binaryExpressionSyntax = child as BinaryExpressionSyntax;
			if (assignmentExpressionSyntax is null && binaryExpressionSyntax is null)
			{
				sortedChildExpressions.Add(new SortedMemberKey(null, memberCount++), (child, childSeparatorToken));
				continue;
			}

			var leftNode = assignmentExpressionSyntax?.Left ?? binaryExpressionSyntax?.Left;
			if (leftNode is null || leftNode is not IdentifierNameSyntax identifierNameSyntax)
			{
				sortedChildExpressions.Add(new SortedMemberKey(null, memberCount++), (child, childSeparatorToken));
				continue;
			}

			var identifierName = identifierNameSyntax.Identifier.ValueText ?? identifierNameSyntax.Identifier.Text;
			if (string.IsNullOrEmpty(identifierName))
			{
				sortedChildExpressions.Add(new SortedMemberKey(null, memberCount++), (child, childSeparatorToken));
				continue;
			}

			var nameOrderedFirstIndex = propertyNamesToOrderFirst is null
				? SortedMemberKey.NameShouldBeAlphabetized
				: Array.FindIndex(propertyNamesToOrderFirst, propertyName => propertyName.Equals(identifierName, propertyNameOrderComparison));

			sortedChildExpressions.Add(new SortedMemberKey(memberCount++, identifierName, nameOrderedFirstIndex), (child, childSeparatorToken));
		}

		var reorderedExpessionsAndSeparators = new List<SyntaxNodeOrToken>();
		foreach (var sortedKVP in sortedChildExpressions)
		{
			var (syntax, separatorToken) = sortedKVP.Value;
			if (separatorToken is null)
			{
				if (syntax.HasTrailingTrivia)
				{
					var trailingTrivia = syntax.GetTrailingTrivia();
					syntax = syntax.WithoutTrailingTrivia();
					separatorToken = SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.CommaToken, trailingTrivia);
				}
				else
				{
					separatorToken = SyntaxFactory.Token(SyntaxTriviaList.Empty, SyntaxKind.CommaToken, new SyntaxTriviaList(SyntaxFactory.ElasticMarker));
				}
			}

			reorderedExpessionsAndSeparators.Add(syntax);
			reorderedExpessionsAndSeparators.Add(separatorToken.Value);
		}

		var reorderedSeparatedList = SyntaxFactory.SeparatedList<ExpressionSyntax>(reorderedExpessionsAndSeparators);
		var newInitializerExpressionSyntax = initializerExpressionSyntax.WithExpressions(reorderedSeparatedList);

		var newRoot = oldRoot.ReplaceNode(initializerExpressionSyntax, newInitializerExpressionSyntax);

#if DEBUG
		var newDocument = document.WithSyntaxRoot(newRoot);
		var newSyntaxTree = await newDocument.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
		var newSyntaxTreeContent = newSyntaxTree?.ToString();
		return newDocument;
#else
		return document.WithSyntaxRoot(newRoot);
#endif
	}

	private static async Task<Document> ReorderTypeMembersAsync(
		Document document,
		string? groupOrderLookupString,
		string? propertyNamesToOrderFirstRawValue,
		bool propertyNamesToOrderFirstAreCaseSensitive,
		ICollection<TypeDeclarationSyntax> typeDeclarationsToFix,
		CancellationToken cancellationToken)
	{
		var oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
		if (oldRoot is null)
			return document;

		// Compute new member order
		var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
		if (semanticModel is null)
			return document;

		var newRoot = oldRoot;
		foreach (var typeDeclaration in typeDeclarationsToFix)
		{
			var typeDeclarationSymbol = semanticModel.GetDeclaredSymbol(typeDeclaration, cancellationToken);
			if (typeDeclarationSymbol is null)
				continue;

			var groupOrder = MemberOrderingOptions.ConvertStringToGroupOrderLookup(groupOrderLookupString) ?? MemberOrderingOptions.DefaultGroupOrder;
			var propertyNamesToOrderFirst = MemberOrderingOptions.GetPropertyNamesToOrderFirst(propertyNamesToOrderFirstRawValue);
			var propertyNameOrderComparison = propertyNamesToOrderFirstAreCaseSensitive
				? StringComparison.Ordinal
				: StringComparison.OrdinalIgnoreCase;

			var memberSymbols = typeDeclarationSymbol.GetMembers();
			var members = typeDeclaration.Members;

			var sortedMembers = new SortedList<SortedMemberKey, MemberDeclarationSyntax>();
			var memberCount = 0;
			foreach (var memberSyntax in members)
			{
				var memberSymbol = memberSymbols.FirstOrDefault(symbol =>
					!symbol.IsImplicitlyDeclared
					&& symbol.Locations.Any(symbolLocation => memberSyntax.Span.Contains(symbolLocation.SourceSpan)));

				if (memberSymbol is null)
				{
					sortedMembers.Add(new SortedMemberKey(null, memberCount++), memberSyntax);
					continue;
				}

				var groupType = MembersOrderedCorrectlyAnalyzer.GetGroupType(memberSymbol);
				if (!memberSymbol.CanBeReferencedByName && groupType != MemberGroupType.Constructors && groupType != MemberGroupType.StaticConstructors)
				{
					sortedMembers.Add(new SortedMemberKey(groupType, memberCount++), memberSyntax);
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
				var nameOrderedFirstIndex = propertyNamesToOrderFirst is null
					? SortedMemberKey.NameShouldBeAlphabetized
					: Array.FindIndex(propertyNamesToOrderFirst, propertyName => propertyName.Equals(memberSymbol.Name, propertyNameOrderComparison));

				var sortKey = matchedGroupIndex > -1
					? new SortedMemberKey(matchedGroupIndex, groupType, memberCount++, memberSymbol.Name, nameOrderedFirstIndex)
					: new SortedMemberKey(groupType, memberCount++);

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

				if (sortedPair.Key.GroupIndex == SortedMemberKey.ImplicitGroupIndex)
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

			newRoot = newRoot.ReplaceNode(typeDeclaration, newTypeDeclaration);
		}

		if (newRoot == oldRoot)
			return document;

#if DEBUG
		var newDocument = document.WithSyntaxRoot(newRoot);
		var newSyntaxTree = await newDocument.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
		var newSyntaxTreeContent = newSyntaxTree?.ToString();
		return newDocument;
#else
		return document.WithSyntaxRoot(newRoot);
#endif
	}

	internal class SortedMemberKey : IComparable<SortedMemberKey>
	{
		internal const int ImplicitGroupIndex = -1;
		internal const int NameShouldBeAlphabetized = -1;

		public SortedMemberKey(
			MemberGroupType? groupType,
			int implicitMemberIndex)
		{
			GroupIndex = ImplicitGroupIndex;
			GroupType = groupType;
			ImplicitMemberIndex = implicitMemberIndex;
			Name = null;
			NameOrderFirstIndex = NameShouldBeAlphabetized;
		}

		// Used for object initializer member assignment sorting
		public SortedMemberKey(
			int memberIndex,
			string name,
			int nameOrderedFirstIndex)
		{
			GroupIndex = ImplicitGroupIndex;
			GroupType = null;
			ImplicitMemberIndex = memberIndex;
			Name = name;
			NameOrderFirstIndex = nameOrderedFirstIndex;
		}

		public SortedMemberKey(
			int groupIndex,
			MemberGroupType groupType,
			int memberIndex,
			string name,
			int nameOrderedFirstIndex)
		{
			GroupIndex = groupIndex;
			GroupType = groupType;
			ImplicitMemberIndex = memberIndex;
			Name = name;
			NameOrderFirstIndex = nameOrderedFirstIndex;
		}

		public int GroupIndex { get; }

		public MemberGroupType? GroupType { get; }

		public int ImplicitMemberIndex { get; }

		public string? Name { get; }

		public int NameOrderFirstIndex { get; }

		public static int Compare(SortedMemberKey x, SortedMemberKey y)
		{
			var comparison = x.GroupIndex.CompareTo(y.GroupIndex);

			if (comparison == 0 && x.NameOrderFirstIndex != y.NameOrderFirstIndex)
			{
				if (y.NameOrderFirstIndex == NameShouldBeAlphabetized)
					return -1; // X has a non-negative name ordered first index so it comes before y which should be alphabetized
				else if (x.NameOrderFirstIndex == NameShouldBeAlphabetized)
					return 1;
				else
					return x.NameOrderFirstIndex.CompareTo(y.NameOrderFirstIndex);
			}

			if (comparison == 0)
				comparison = string.CompareOrdinal(x.Name, y.Name);

			if (comparison == 0)
				comparison = x.ImplicitMemberIndex.CompareTo(y.ImplicitMemberIndex);

			return comparison;
		}

		public int CompareTo(SortedMemberKey other) => Compare(this, other);
	}
}
