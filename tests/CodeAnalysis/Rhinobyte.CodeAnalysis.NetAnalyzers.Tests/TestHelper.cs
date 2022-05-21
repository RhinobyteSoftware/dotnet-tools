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
		string? subdirectoryName = null,
		[CallerMemberName] string? fileNamePrefix = null)
	{
		var filePath = subdirectoryName is null
			? Path.Combine(Directory.GetCurrentDirectory(), "TestCaseFiles", $"{fileNamePrefix}{extension}")
			: Path.Combine(Directory.GetCurrentDirectory(), "TestCaseFiles", subdirectoryName, $"{fileNamePrefix}{extension}");

		return await File.ReadAllTextAsync(filePath);
	}

	public static async Task<string> GetTestCodeFixResultFileAsync(CancellationToken cancellationToken, string? subdirectoryName = null, [CallerMemberName] string? fileNamePrefix = null)
		=> await GetTestCaseFileAsync(cancellationToken, ".codefixresult.cs", subdirectoryName, fileNamePrefix);

	public static async Task<string> GetTestInputFileAsync(CancellationToken cancellationToken, string? subdirectoryName = null, [CallerMemberName] string? fileNamePrefix = null)
		=> await GetTestCaseFileAsync(cancellationToken, ".input.cs", subdirectoryName, fileNamePrefix);
}
