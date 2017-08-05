using UnityEngine;
using System;
using System.Collections;

public enum DataType
{

}

public class DialogueData{
	public enum CommandType {App = 0, Disapp = 1, BGM = 2, BG = 3, SE = 4, Script = 5, Battle = 6, Map = 7,
	//이하는 SubType이 필요없는 것들. 순서 중요하므로 바꿀 거면 신중할 것
	Adv = 8, Title = 9, FIO = 10, Else};
	public CommandType Command;

	bool isEffect;
	string nameInCode;
	string emotion;
	string name;
	string dialogue;

	string commandSubType;

	bool isAdventureObject;
	string objectName;
	string objectSubName;

	public bool IsEffect() { return isEffect; }
	public string GetNameInCode() { return nameInCode; }
	public string GetEmotion() { return emotion; }
	public string GetName() { return name; }
	public string GetDialogue() { return dialogue; }
	public string GetCommandSubType() { return commandSubType; }
	public bool IsAdventureObject() { return isAdventureObject; }
	public string GetObjectName() { return objectName; }
	public string GetObjectSubName() { return objectSubName; }

	public DialogueData (string unparsedDialogueDataString){
		StringParser parser = new StringParser(unparsedDialogueDataString, '\t');
		string inputType = parser.Consume();
		if(inputType == "*"){
			isEffect = true;
			isAdventureObject = false;
			Command = parser.ConsumeEnum<CommandType>();

			if((int)Command <= 7)
				commandSubType = parser.Consume();
			if(Command == CommandType.App)
				nameInCode = parser.Consume();
		}else if (inputType == "**"){
			// adventure objects.
			isEffect = false;
			isAdventureObject = true;
			objectName = parser.Consume();
			objectSubName = parser.Consume();
		}else{
			parser.ResetIndex();
			isEffect = false;
			isAdventureObject = false;
			nameInCode = parser.Consume();
			emotion = parser.Consume();
			name = parser.Consume();
			dialogue = parser.Consume();
		}
		/*catch (Exception e){
			Debug.LogError("Parse error with " + unparsedDialogueDataString);
			Debug.LogException(e);
			throw e;
		}*/
	}
}
