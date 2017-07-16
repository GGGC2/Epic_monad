using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameData{
	public class PartyData : MonoBehaviour{
		public static int level;
		public static int exp;
		public static int reqExp;

		public static void SetReqExp(){
			reqExp = (int)Mathf.Pow((0.117f*level)+0.883f, 3)*100;
		}

		public static void AddExp(int point){
			exp += point;
			if(exp >= reqExp){
				level += 1;
				exp -= reqExp;
				SetReqExp();
			}
		}
	}

	public class SceneData {
		public static string dialogueName;
		public static string stageName;
	}
}