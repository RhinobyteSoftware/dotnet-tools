using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rhinobyte.CodeAnalysis.NetAnalyzers.Tests.TestCaseFiles;

public class ReorderParametersTestClass(
	string alpha,
	string zulu)
{
	public ReorderParametersTestClass(
		string alpha,
		string bravo,
		string charlie)
		: this(alpha, "zulu")
	{ }

	public ReorderParametersTestClass(
		string alpha,
		string bravo,
		string charlie,
		string delta,
		string echo)
		: this(alpha, "zulu")
	{ }

	public void MethodOne()
	{
		// No-op
	}

	public void MethodTwo(
		int alpha,
		int bravo)
	{
		// No-op
	}

	public void MethodThree(int alpha, int zulu)
	{
		// No-op
	}
}
