using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rhinobyte.CodeAnalysis.NetAnalyzers.Tests.TestCaseFiles;

public abstract class ExampleClassWithIncorrectGroupOrdering
{
	public const string ConstantA = nameof(ConstantA);
	public const string ConstantC = nameof(ConstantC);
	public const string ConstantE = nameof(ConstantE);
	public const string ConstantD = nameof(ConstantD);
	public const string ConstantZ = nameof(ConstantZ);
	public const string ConstantB = nameof(ConstantB);

	public static bool StaticPropertyA { get; } = true;
	public static bool StaticPropertyC { get; } = true;
	public static bool StaticPropertyE { get; } = true;
	public static bool StaticPropertyD { get; } = true;
	public static bool StaticPropertyZ { get; } = true;
	public static bool StaticPropertyB { get; } = true;

	public ExampleClassWithIncorrectGroupOrdering()
	{

	}

	public int PropertyA { get; set; }
	public int PropertyC { get; set; }
	public int PropertyE { get; set; }
	public int PropertyD { get; set; }
	public int PropertyZ { get; set; }
	public int PropertyB { get; set; }

	public Task<bool> MethodA(CancellationToken cancellationToken)
	{
		return Task.FromResult(true);
	}

	public virtual int MethodC() => 1;

	public abstract void MethodE();

	public static Task<bool> MethodD(CancellationToken cancellationToken)
	{
		return Task.FromResult(true);
	}

	public static int MethodZ() => 1;

	public abstract void MethodB();
}
