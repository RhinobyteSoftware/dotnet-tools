using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace Rhinobyte.CodeAnalysis.NetAnalyzers;

internal static class DiagnosticDescriptorHelper
{
	internal const string DesignCategory = "Design";

	public static DiagnosticDescriptor Create(
			string id,
			string category,
			LocalizableString? description,
			DiagnosticSeverity diagnosticSeverity,
			LocalizableString messageFormat,
			LocalizableString title,
			bool isEnabledByDefault = true,
			bool isReportedAtCompilationEnd = false,
			params string[] additionalCustomTags)
	{
#pragma warning disable CA1308 // Normalize strings to uppercase - use lower case ID in help link
		var helpLink = $"https://github.com/RhinobyteSoftware/dotnet-tools/docs/rules/{id.ToLowerInvariant()}.md";
#pragma warning restore CA1308 // Normalize strings to uppercase

		string[]? customTags = null;
		if (isReportedAtCompilationEnd)
			customTags = new string[] { WellKnownDiagnosticTags.CompilationEnd };

		if (additionalCustomTags.Length > 0)
			customTags = customTags is null ? additionalCustomTags : customTags.Concat(additionalCustomTags).ToArray();

		return new DiagnosticDescriptor(id, title, messageFormat, category, diagnosticSeverity, isEnabledByDefault, description, helpLink, customTags ?? Array.Empty<string>());
	}

	internal static class WellKnownDiagnosticTags
	{
		public const string CompilationEnd = nameof(CompilationEnd);
	}
}
