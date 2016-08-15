using UnityEngine;
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
		string strValue = Consume();
		try
		{
			return bool.Parse(strValue);
		}
		catch (FormatException e)
		{
			Debug.LogWarning("Cannot parse boolean value " + strValue);
			return false;
		}
	}

	public int ConsumeInt()
	{
		string strValue = Consume();
		try
		{
			return Int32.Parse(strValue);
		}
		catch (FormatException e)
		{
			Debug.LogWarning("Cannot parse integer value " + strValue);
			return -1;
		}
	}

	public float ConsumeFloat()
	{
		string strValue = Consume();
		try
		{
			return Single.Parse(Consume());
		}
		catch (FormatException e)
		{
			Debug.LogWarning("Cannot parse float value " + strValue);
			return -1;
		}
	}

	public T ConsumeEnum<T>()
	{
		string beforeParsed = Consume();
		try 
		{
			return (T)Enum.Parse(typeof(T), beforeParsed);
		}
		catch (ArgumentException e)
		{
			Debug.LogWarning("Invalid enum value " + beforeParsed + " : " + typeof(T).FullName);
			return default(T); // null
		}
	}
}
