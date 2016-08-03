using System;

public class CommaStringParser
{
	string[] origin = null;
	int index = 0;

	public CommaStringParser(string line)
	{
		origin = line.Split(',');
		index = 0;
	}

	public string Consume()
	{
		index += 1;
		return origin[index - 1];
	}

	public bool ConsumeBool()
	{
		return bool.Parse(Consume());
	}

	public int ConsumeInt()
	{
		return Int32.Parse(Consume());
	}

	public float ConsumeFloat()
	{
		return Single.Parse(Consume());
	}

	public T ConsumeEnum<T>()
	{
		string beforeParsed = Consume();
		return (T)Enum.Parse(typeof(T), beforeParsed);
	}
}
