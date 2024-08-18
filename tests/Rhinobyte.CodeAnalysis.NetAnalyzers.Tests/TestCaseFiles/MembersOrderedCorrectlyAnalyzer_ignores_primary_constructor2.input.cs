using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rhinobyte.CodeAnalysis.NetAnalyzers.Tests.TestCaseFiles;

public class ExampleClassWithIncorrectAlphabeticalOrdering(
	string primaryConstructorParam)
{
	public const string ConstantOne = nameof(ConstantOne);

	// Field and constant should not be flagged as out of order due to the primary constructor above them
	private readonly string _fieldA;

	public ExampleClassWithIncorrectAlphabeticalOrdering(string primaryConstructorParam, string secondParam)
		: this(primaryConstructorParam)
	{

	}
}
