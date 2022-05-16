using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rhinobyte.CodeAnalysis.NetAnalyzers.Tests.TestCaseFiles;

public abstract partial class ExampleClassWithIncorrectGroupOrdering
{
	public const string Random = nameof(Random);
}

public abstract partial class ExampleClassWithIncorrectGroupOrdering
{
	public const string ConstantA = nameof(ConstantA);
	public const string Random2 = nameof(Random2);

	public static bool StaticPropertyA { get; } = true;

	public int PropertyA { get; set; }

	public Task<bool> MethodA(CancellationToken cancellationToken)
	{
		return Task.FromResult(true);
	}
}

public abstract partial class ExampleClassWithIncorrectGroupOrdering
{
	public const string ConstantB = nameof(ConstantB);
	public const string ConstantC = nameof(ConstantC);
	public const string ConstantD = nameof(ConstantD);
	public const string ConstantE = nameof(ConstantE);
	public const string ConstantZ = nameof(ConstantZ);

    public static bool StaticPropertyB { get; } = true;
	public static bool StaticPropertyC { get; } = true;
	public static bool StaticPropertyD { get; } = true;
	public static bool StaticPropertyE { get; } = true;
	public static bool StaticPropertyZ { get; } = true;

	public ExampleClassWithIncorrectGroupOrdering()
	{

	}

    public int PropertyB { get; set; }
	public int PropertyC { get; set; }
	public int PropertyD { get; set; }
	public int PropertyE { get; set; }
	public int PropertyZ { get; set; }

	public abstract void MethodB();

	public virtual int MethodC() => 1;

	public static Task<bool> MethodD(CancellationToken cancellationToken)
	{
		return Task.FromResult(true);
	}

	public abstract void MethodE();

	public static int MethodZ() => 1;
}
