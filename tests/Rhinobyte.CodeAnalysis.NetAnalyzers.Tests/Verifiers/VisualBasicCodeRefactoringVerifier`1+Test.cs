﻿using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.VisualBasic.Testing;

namespace Rhinobyte.CodeAnalysis.NetAnalyzers.Tests;

public static partial class VisualBasicCodeRefactoringVerifier<TCodeRefactoring>
	where TCodeRefactoring : CodeRefactoringProvider, new()
{
	public class Test : VisualBasicCodeRefactoringTest<TCodeRefactoring, DefaultVerifier>
	{
	}
}