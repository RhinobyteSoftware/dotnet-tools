using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rhinobyte.CodeAnalysis.NetAnalyzers.Tests.TestCaseFiles;

public class ExampleClassWithCaseDifferentPropertyNames
{
	public long CombinedUsageCount { get; set; }
	public long DataPageCount { get; set; }
	public string DatabaseName { get; set; }
}
