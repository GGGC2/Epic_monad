using UnityEngine;

namespace WorldMap
{
public class StoryInfo
{
	public string storyName;
	public string dialogueName;

	public StoryInfo(string line)
	{
		string[] tokens = line.Split(',');

		if (tokens.Length != 2)
		{
			Debug.LogError("Invalid file story info " + tokens.Length);
			return;
		}

		this.storyName = tokens[0];
		this.dialogueName = tokens[1];
	}
}
}
