using Microsoft.CodeAnalysis;
using System;

namespace Rhinobyte.CodeAnalysis.NetAnalyzers;

/// <summary>
/// Helper methods for working with symbols, syntax nodes, etc
/// </summary>
internal static class SyntaxHelper
{
	/// <summary>
	/// Returns true if <paramref name="positionToLookFor"/> is for the same <see cref="FileLinePositionSpan.Path"/> as <paramref name="spanToSearch"/>
	/// and the LinePosition being looked for is fully within the span to search.
	/// </summary>
	internal static bool IsPositionSpanWithin(FileLinePositionSpan positionToLookFor, FileLinePositionSpan spanToSearch)
	{
		return positionToLookFor.Path.Equals(spanToSearch.Path, StringComparison.Ordinal)
			&& positionToLookFor.StartLinePosition > spanToSearch.StartLinePosition
			&& positionToLookFor.EndLinePosition < spanToSearch.EndLinePosition;
	}
}
