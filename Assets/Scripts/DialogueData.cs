using UnityEngine;
using System.Collections;

public enum DataType
{
	
}

public class DialogueData{

	bool isEffect;
	string nameInCode;
	string emotion;
	string name;
	string dialogue;
	
	string commandType;
	string commandSubType;

	bool isAdventureObject;
	string objectName;
	string objectSubName;
	
	public bool IsEffect() { return isEffect; }
	public string GetNameInCode() { return nameInCode; }
	public string GetEmotion() { return nameInCode; }
	public string GetName() { return name; }
	public string GetDialogue() { return dialogue; }
	public string GetCommandType() { return commandType; }
	public string GetCommandSubType() { return commandSubType; }
	public bool IsAdventureObject() { return isAdventureObject; }
	public string GetObjectName() { return objectName; }
	public string GetObjectSubName() { return objectSubName; } 
	
	public DialogueData (string unparsedDialogueDataString)
	{
		string[] stringList = unparsedDialogueDataString.Split('\t');

		if (stringList[0] == "*") // effects.
		{
			isEffect = true;
			isAdventureObject = false;
			commandType = stringList[1];
			if (commandType == "appear")
			{
				commandSubType = stringList[2];
				nameInCode = stringList[3];
			}
			else if (commandType == "disappear")
			{
				commandSubType = stringList[2];
			}
			else if (commandType == "adv_start")
			{
				// nothing.
			}
			else if (commandType == "load_scene")
			{
				// load next scene.
				commandSubType = stringList[2];
			}
			else if (commandType == "load_script")
			{
				// load next script.
				commandSubType = stringList[2];
			}
			else
			{
				Debug.LogError("undefined effectType : " + stringList[1]);
			}
		}
		else if (stringList[0] == "**") // adventure objects.
		{
			isEffect = false;
			isAdventureObject = true;
			objectName = stringList[1];
			objectSubName = stringList[2];
		}
		else
		{
			isEffect = false;
			isAdventureObject = false;
			nameInCode = stringList[0];
			emotion = stringList[1];
			name = stringList[2];
			dialogue = stringList[3];
		}
	}
}
