using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rhinobyte.CodeAnalysis.NetAnalyzers.Tests.TestCaseFiles
{
	public class ExampleClassWithIncorrectAlphabeticalOrdering(
		string primaryConstructorParam)
	{
		public static readonly TimeSpan TimeSpanConstant = TimeSpan.FromMinutes(1);
		public const string ConstantOne = nameof(ConstantOne);

		private readonly string _fieldA;
		private bool _fieldE;
		private readonly int _fieldB;
		private string _fieldZ;

		public ExampleClassWithIncorrectAlphabeticalOrdering(string primaryConstructorParam, string secondParam)
			: this(primaryConstructorParam)
		{

		}

		public bool IsNotValid { get; set; }

		public bool IsEnabled { get; set; }

		public Task<bool> SomethingElseAsync(CancellationToken cancellationToken)
		{
			return Task.FromResult(true);
		}

		public static async Task DoSomethingAsync(CancellationToken cancellationToken)
		{

		}

		public string OutOfOrderProperty { get; set; }
	}
}
