using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rhinobyte.CodeAnalysis.NetAnalyzers.Tests.TestCaseFiles;

public enum ExampleEnumWithIncorrectAlphabeticalOrdering
{
	Alpha = 0,
	Bravo = 2,
	Charlie = 1,
	Delta = 4,
	Echo = 3,
	Zulu = 5
}

public enum ExampleEnumTwo
{
	/// <summary>
	/// Comment for Alpha
	/// </summary>
	Alpha = 1,

	/// <summary>
	/// Comment for Bravo
	/// </summary>
	Bravo = 3,

	/// <summary>
	/// Comment for Charlie
	/// </summary>
	Charlie = 2,

	/// <summary>
	/// Comment for Delta
	/// </summary>
	Delta = 5,

	/// <summary>
	/// Comment for Echo
	/// </summary>
	Echo = 4,

	/// <summary>
	/// Comment for Zulu
	/// </summary>
	Zulu = 6
}
