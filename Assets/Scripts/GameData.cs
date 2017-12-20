using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameData{
	public class PartyData{
		public static int level;
		public static int exp;
		public static int reqExp;

		public static void SetLevel(int number, bool plus = false){
			if(plus) {level += number;}
			else {level = number;}
		}

		public static void SetReqExp(){ reqExp = (int)(Mathf.Pow((0.117f*level)+0.883f, 3)*100); }

		public static void CheckLevelData(){
			if(level == 0) {level = 1;}
			if(reqExp == 0) {SetReqExp();}
		}

		public static void SetDefault(){
			level = 1;
			exp = 0;
			reqExp = 100;
		}

		public static void AddExp(int point){
			exp += point;
			if(exp >= reqExp){
				SetLevel(1, true);
				exp -= reqExp;
				SetReqExp();
			}
		}
		
		public static int MaxEther{
			get{
				return level + 15;
			}
		}
	}

	public class SceneData {
		public static string dialogueName;
		public static int stageNumber;
        public static bool isDialogue;
        public static bool isTestMode = false;
        public static bool isStageMode = false;
	}

	public class GlobalData{
		public static List<GlossaryData> GlossaryDataList = new List<GlossaryData>();
		public static void SetGlossaryDataList(){
			if(GlossaryDataList.Count == 0){
				GlossaryDataList = Parser.GetParsedData<GlossaryData>();
			}
		}

		public static void ViewAllGlossaryData(){
			foreach(GlossaryData data in GlossaryDataList){
				Debug.Log("" + data.Type + data.index + " : LEVEL" + data.level);
			}
		}
	}
}