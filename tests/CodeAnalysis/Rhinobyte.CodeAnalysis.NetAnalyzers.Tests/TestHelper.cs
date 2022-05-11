#nullable enable

using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Rhinobyte.CodeAnalysis.NetAnalyzers.Tests;

internal static class TestHelper
{
	public static async Task<string> GetTestCaseFileAsync(
		CancellationToken cancellationToken,
		string extension,
		[CallerMemberName] string? fileNamePrefix = null)
	{
		return await File.ReadAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(), "TestCaseFiles", $"{fileNamePrefix}{extension}"));
	}

	public static async Task<string> GetTestCodeFixResultFileAsync(CancellationToken cancellationToken, [CallerMemberName] string? fileNamePrefix = null)
		=> await GetTestCaseFileAsync(cancellationToken, ".codefixresult.cs", fileNamePrefix);

	public static async Task<string> GetTestInputFileAsync(CancellationToken cancellationToken, [CallerMemberName] string? fileNamePrefix = null)
		=> await GetTestCaseFileAsync(cancellationToken, ".input.cs", fileNamePrefix);
}
