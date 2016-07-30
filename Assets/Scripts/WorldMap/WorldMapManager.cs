using System.Collections.Generic;
using UnityEngine;

namespace WorldMap
{
public class WorldMapManager : MonoBehaviour
{
	public static List<StoryInfo> storyInfos = new List<StoryInfo>();
	public static string currentStory = "";

	public static string GetNextDialogue()
	{
		foreach (StoryInfo storyInfo in storyInfos)
		{
			if (storyInfo.storyName == currentStory)
			{
				return storyInfo.dialogueName;
			}
		}

		return null;
	}

	void Awake()
	{
		if (storyInfos.Count == 0)
		{
			Debug.Log("WorldMapManager initialized");
			storyInfos = Parser.GetParsedStoryInfo();
			Debug.Assert(storyInfos.Count != 0);
		}

		if (currentStory == "")
		{
			currentStory = storyInfos[0].storyName;
			Debug.Log("Set current story to " + currentStory);
		}
	}

	public static void ToNextDialogue()
	{
		string nextDialogue = GetNextDialogue();
		Debug.Assert(nextDialogue != null);

		FindObjectOfType<SceneLoader>().LoadNextScript(nextDialogue);
	}
}
}
