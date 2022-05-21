using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rhinobyte.CodeAnalysis.NetAnalyzers.Tests.TestCaseFiles.ObjectInitializerMemberOrderTests;

public class TestService
{
	public void DoSomething()
	{
		var poco3 = new ExamplePoco();

		var poco4 = new ExamplePoco()
		{
			Bravo = "Bravo",
			Zulu = "Zulu",
			Charlie = "Charlie",
			Golf = "Golf",
			Id = 2,
			Alpha = "Alpha",
		};

		var poco5 = new ExamplePoco()
		{
			Alpha = "Alpha",
			Bravo = "Bravo",
			Charlie = "Charlie",
			Golf = "Golf",
			Id = 2,
			Zulu = "Zulu",
		};
	}

	public async Task DoSomethingAsync(CancellationToken cancellationToken)
	{
		var poco1 = new ExamplePoco();

		var poco2 = new ExamplePoco()
		{
			Bravo = "Bravo",
			Zulu = "Zulu",
			Charlie = "Charlie",
			Golf = "Golf",
			Id = 2,
			Alpha = "Alpha"
		};
	}

	public class ExamplePoco
	{
		public string Alpha { get; set; }
		public string Bravo { get; set; }
		public string Charlie { get; set; }
		public string Golf { get; set; }
		public int Id { get; set; }
		public string Zulu { get; set; }
	}
}
