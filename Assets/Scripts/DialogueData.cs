using UnityEngine;
using System;
using System.Collections;

public enum DataType
{

}

public class DialogueData{
	public enum CommandType {App = 0, Disapp = 1, BGM = 2, BG = 3, SE = 4, Script = 5, Battle = 6, Map = 7, Glos = 8,
	//이하는 SubType이 필요없는 것들. 할당된 int의 범위로 불러오는 경우가 있기 때문에, SubType이 있는 것이 항상 없는 것보다 먼저 와야 한다.
	Adv = 9, Title = 10, FO = 11, FI = 12, Gray = 13, Colorful = 14, Else};
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
	int glossaryIndex;
	int glossarySetLevel;

	public bool IsEffect() { return isEffect; }
	public string GetNameInCode() { return nameInCode; }
	public string GetEmotion() { return emotion; }
	public string GetName() { return name; }
	public string GetDialogue() { return dialogue; }
	public string GetCommandSubType() { return commandSubType; }
	public bool IsAdventureObject() { return isAdventureObject; }
	public string GetObjectName() { return objectName; }
	public string GetObjectSubName() { return objectSubName; }
	public int GetGlossaryIndex() { return glossaryIndex; }
	public int GetGlossarySetLevel() { return glossarySetLevel; }

	public DialogueData (string unparsedDialogueDataString){
		StringParser parser = new StringParser(unparsedDialogueDataString, '\t');
		string inputType = parser.Consume();
		if(inputType == "*"){
			isEffect = true;
			isAdventureObject = false;
			Command = parser.ConsumeEnum<CommandType>();

			if((int)Command < (int)CommandType.Adv) {commandSubType = parser.Consume();}

			if(Command == CommandType.App) {nameInCode = parser.Consume();}
			else if(Command == CommandType.Glos){
				glossaryIndex = parser.ConsumeInt();
				glossarySetLevel = parser.ConsumeInt();
			}
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
			dialogue = dialogue.Replace ("^", "\n");
		}
	}
}
