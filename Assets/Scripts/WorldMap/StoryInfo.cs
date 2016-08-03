using UnityEngine;

namespace WorldMap
{
public class StoryInfo
{
	public string storyName;
	public string dialogueName;

	public StoryInfo(string line)
	{
		CommaStringParser commaParser = new CommaStringParser(line);

		this.storyName = commaParser.Consume();
		this.dialogueName = commaParser.Consume();
	}
}
}
