using UnityEngine;
using System;

public class StringParser{
	public string[] origin = null;
	public int index = 0;

	public StringParser(string line, char separator){
		origin = line.Split(separator);
	}

	public void ResetIndex(){
		index = 0;
	}

	public string Consume(){
		index += 1;
		try{
			return origin[index - 1];
		}catch{
			Debug.LogError((index-1) + "번째 항목을 읽을 수 없습니다!");
			Debug.LogError("행의 맨 앞 항목 : " + origin[0] + " / 마지막 항목 : " + origin[index-2]);
			return "";
		}
	}

	public bool ConsumeBool(){
		string strValue = Consume();
		try{
			return bool.Parse(strValue);
		}catch (FormatException e){
			Debug.LogError("Cannot parse boolean value " + strValue);
			return false;
		}
	}

	 // true / false / none 처리용. 디폴트로 인식할 스트링과 그 결과값을 받는다.
	public bool ConsumeBool(string defaultValue, bool result){
		string strValue = Consume();
		try{
			if (strValue == defaultValue) return result;
			return bool.Parse(strValue);
		}catch{
			Debug.LogError("Cannot parse boolean value of : " + strValue);
			return false;
		}
	}

	public int ConsumeInt(){
		string strValue = Consume();
		try{
			return Int32.Parse(strValue);
		}catch{
			Debug.LogError("Cannot parse integer value of : " + strValue);
			return -1;
		}
	}

	public float ConsumeFloat(){
		string strValue = Consume();
		try{
			return Single.Parse(strValue);
		}catch{
			Debug.LogError("Cannot parse float value of : " + strValue);
			return -1;
		}
	}

	// 기본값이 있는 float 변수 파싱용. ConsumeString과 동일.
	public float ConsumeFloat(string defaultValue, float f){
		string strValue = Consume();
		try
		{	
			if (strValue == defaultValue) return f;
			return Single.Parse(strValue);
		}catch{
			Debug.LogError("Cannot parse float value of : " + strValue);
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
			if(beforeParsed == "") {beforeParsed = "(empty)";}
			Debug.LogError("잘못된 Enum Parsing " + beforeParsed + " : " + typeof(T).FullName + " / " + 
				origin[0] + ", " + origin[1] + ", " + origin[2] + "... / " + index + "번째");
			return default(T); // null
		}
	}
}
