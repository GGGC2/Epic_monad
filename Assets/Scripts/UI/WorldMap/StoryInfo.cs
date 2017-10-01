using UnityEngine;

namespace WorldMap
{
	public class StoryInfo
	{
		public string storyName;
		public string dialogueName;

		public StoryInfo(string line){
			StringParser commaParser = new StringParser(line, ',');

			this.storyName = commaParser.Consume();
			this.dialogueName = commaParser.Consume();
		}
	}
}