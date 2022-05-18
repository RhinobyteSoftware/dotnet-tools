using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.CodeAnalysis.NetAnalyzers.Utilities;

namespace Rhinobyte.CodeAnalysis.NetAnalyzers.Tests.Utilities;

[TestClass]
public class AnalyzerOptionsExtensionsTests
{
	[TestMethod]
	public void FindGetOrComputeMethod_does_not_return_null()
	{
		var getOrComputeMethodInfo = AnalyzerOptionsExtensions.FindGetOrComputeMethod();
		getOrComputeMethodInfo.Should().NotBeNull();
	}
}
