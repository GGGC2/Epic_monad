using UnityEngine;
using System;

public class StringParser{
	string[] origin = null;
	int index = 0;

	public StringParser(string line, char separator){
		origin = line.Split(separator);
		index = 0;
	}

	public void ResetIndex(){
		index = 0;
	}

	public string Consume(){
		index += 1;
		return origin[index - 1];
	}

	public bool ConsumeBool(){
		string strValue = Consume();
		try
		{
			return bool.Parse(strValue);
		}
		catch (FormatException e)
		{
			Debug.LogError("Cannot parse boolean value " + strValue);
			return false;
		}
	}

	 // true / false / none 처리용. 디폴트로 인식할 스트링과 그 결과값을 받는다.
	public bool ConsumeBool(string defaultValue, bool result)
	{
		string strValue = Consume();
		try
		{
			if (strValue == defaultValue) return result;
			return bool.Parse(strValue);
		}
		catch (FormatException e)
		{
			Debug.LogError("Cannot parse boolean value " + strValue);
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
			Debug.LogError("Cannot parse integer value " + strValue);
			return -1;
		}
	}

	public float ConsumeFloat()
	{
		string strValue = Consume();
		try
		{
			return Single.Parse(strValue);
		}
		catch (FormatException e)
		{
			Debug.LogError("Cannot parse float value " + strValue);
			return -1;
		}
	}

	// 기본값이 있는 float 변수 파싱용. ConsumeString과 동일.
	public float ConsumeFloat(string defaultValue, float f)
	{
		string strValue = Consume();
		try
		{	
			if (strValue == defaultValue) return f;
			return Single.Parse(strValue);
		}
		catch (FormatException e)
		{
			Debug.LogError("Cannot parse float value " + strValue);
			return -1;
		}
	}

	public T ConsumeEnum<T>(){
		string beforeParsed = Consume();
		try{
			if (beforeParsed == "X" || beforeParsed == "O") return (T)Enum.Parse(typeof(T), "None");
			else if (beforeParsed == "-") return (T)Enum.Parse(typeof(T), "Once");
			else return (T)Enum.Parse(typeof(T), beforeParsed);
		}catch (ArgumentException e){
			Debug.LogError("Invalid enum value " + beforeParsed + " : " + typeof(T).FullName);
			return default(T); // null
		}
	}
}
