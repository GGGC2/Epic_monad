using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Save;
using System;

namespace WorldMap{
	public class WorldMapManager : MonoBehaviour{
		public static List<StoryInfo> storyInfos = new List<StoryInfo>();

			public static string currentStory = "";

		public static string GetNextDialogue(){
			foreach (StoryInfo storyInfo in storyInfos){
				if (storyInfo.storyName == currentStory)
					return storyInfo.dialogueName;
			}

			return null;
		}

		public static void ToSkillTree(){
			SceneManager.LoadScene("SkillTree");
		}


		void Awake(){
			if (storyInfos.Count == 0){
				storyInfos = Parser.GetParsedStoryInfo();
				Debug.Assert(storyInfos.Count != 0);
			}
		}

		public static void ToNextDialogue(){
			string nextDialogue = GetNextDialogue();
			Debug.Assert(nextDialogue != null);

			FindObjectOfType<SceneLoader>().LoadNextDialogueScene(nextDialogue);
		}
	}
}