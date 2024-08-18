public class ExampleClassWithMethodsToOrder
{
	public int Alpha()
	{
		return 1;
	}

	public string InstanceMethodToPutAtTheTop()
	{
		return string.Empty;
	}

	public long Bravo()
	{
		return 2;
	}

	public static void StaticMethodToPutAtTheTop()
	{ }

	public string OtherInstanceMethodToPutAtTheTop()
		=> string.Empty;
}
