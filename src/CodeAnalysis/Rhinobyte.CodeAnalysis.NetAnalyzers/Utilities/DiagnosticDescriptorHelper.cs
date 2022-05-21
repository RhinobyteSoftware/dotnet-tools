using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace Rhinobyte.CodeAnalysis.NetAnalyzers.Utilities;

public static class DiagnosticDescriptorHelper
{
	internal const string DesignCategory = "Design";
	internal const string MaintainabilityCategory = "Maintainability";

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
		_ = id ?? throw new ArgumentNullException(nameof(id));

#pragma warning disable CA1308 // Normalize strings to uppercase - use lower case ID in help link
		var helpLink = $"https://github.com/RhinobyteSoftware/dotnet-tools/blob/main/docs/codeanalysis/rules/{id.ToLowerInvariant()}.md";
#pragma warning restore CA1308 // Normalize strings to uppercase

		string[]? customTags = null;
		if (isReportedAtCompilationEnd)
			customTags = new string[] { WellKnownDiagnosticTags.CompilationEnd };

#pragma warning disable CA1062 // Validate arguments of public methods
		if (additionalCustomTags.Length > 0)
			customTags = customTags is null ? additionalCustomTags : customTags.Concat(additionalCustomTags).ToArray();
#pragma warning restore CA1062 // Validate arguments of public methods

		return new DiagnosticDescriptor(id, title, messageFormat, category, diagnosticSeverity, isEnabledByDefault, description, helpLink, customTags ?? Array.Empty<string>());
	}

	internal static class WellKnownDiagnosticTags
	{
		public const string CompilationEnd = nameof(CompilationEnd);
	}
}
