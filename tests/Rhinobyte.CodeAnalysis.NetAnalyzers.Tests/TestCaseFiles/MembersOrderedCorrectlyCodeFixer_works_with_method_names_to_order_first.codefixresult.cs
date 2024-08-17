public class ExampleClassWithMethodsToOrder
{

	public static void StaticMethodToPutAtTheTop()
	{ }

	public string OtherInstanceMethodToPutAtTheTop()
		=> string.Empty;

	public string InstanceMethodToPutAtTheTop()
	{
		return string.Empty;
	}

    public int Alpha()
	{
		return 1;
	}

	public long Bravo()
	{
		return 2;
	}
}
