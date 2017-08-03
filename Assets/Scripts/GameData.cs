using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameData{
	public class PartyData{
		public static int level;
		public static int exp;
		public static int reqExp;

		public static void SetReqExp(){ reqExp = (int)(Mathf.Pow((0.117f*GetLevel())+0.883f, 3)*100); }

		public static void CheckLevelZero(){
			if(GetLevel() == 0){ SetDefault(); }
		}

		public static void SetDefault(){
			Debug.Log("Set Default LEVEL 1");
			level = 1;
			exp = 0;
			reqExp = 100;
		}

		public static void AddExp(int point){
			exp += point;
			if(exp >= reqExp){
				level += 1;
				exp -= reqExp;
				SetReqExp();
			}
		}

		public static int GetLevel(){ return level; }
	}

	public class SceneData {
		public static string dialogueName;
		public static int stageNumber;
        public static bool isDialogue;
        public static bool isTestMode = false;
        public static bool isStageMode = false;
	}
}