using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Rhinobyte.CodeAnalysis.NetAnalyzers;

/// <summary>
/// Code analzyer for design time checks that type members are ordered alphabetically by member name for their respective groups
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AlphabeticalMemberOrderAnalyzer : DiagnosticAnalyzer
{
	/// <summary>
	/// The diagnostic rule id for the analzyer
	/// </summary>
	public const string DiagnosticId = "RBCS0001";

	// You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
	// See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization

	private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorHelper.Create(
		DiagnosticId,
		DiagnosticDescriptorHelper.DesignCategory,
		new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources)),
		DiagnosticSeverity.Info,
		new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources)),
		new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources)),
		isEnabledByDefault: true
	);

	/// <inheritdoc />
	public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

	private static void AnalyzeSymbol(SymbolAnalysisContext context)
	{
		// TODO: Replace the following code with your own analysis, generating Diagnostic objects for any issues you find
		var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

		// Find just those named type symbols with names containing lowercase letters.
		if (namedTypeSymbol.Name.ToCharArray().Any(char.IsLower))
		{
			// For all such symbols, produce a diagnostic.
			var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);

			context.ReportDiagnostic(diagnostic);
		}
	}

	/// <inheritdoc />
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1062:Validate arguments of public methods", Justification = "Unnecessary in code analyzer extension")]
	public override void Initialize(AnalysisContext context)
	{
		context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
		context.EnableConcurrentExecution();

		// TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
		// See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
		context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
	}
}
