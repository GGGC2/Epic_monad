using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CustomWorldText : MonoBehaviour
{
	public string text = "013";
	public Align align;

	public Sprite sprite0;
	public Sprite sprite1;
	public Sprite sprite2;
	public Sprite sprite3;
	public Sprite sprite4;
	public Sprite sprite5;
	public Sprite sprite6;
	public Sprite sprite7;
	public Sprite sprite8;
	public Sprite sprite9;
	public Sprite spriteMinus;
	public Sprite spritePlus;

	private static int gap = 50;
	private List<GameObject> characterInstances = new List<GameObject>();

	public enum Align
	{
		LEFT,
		MIDDLE,
		RIGHT
	}

	public void ApplyText()
	{
		DestroyAllChilds();
		GenerateTextInstances();
		RePosition();
	}

	private void DestroyAllChilds()
	{
		foreach(var characterInstance in characterInstances)
		{
			Destroy(characterInstance.gameObject);
		}
		characterInstances.Clear();
	}

	private void GenerateTextInstances()
	{
		foreach(var character in text) {
			// Debug.Log(character);

			var gameObject = new GameObject(character.ToString(), typeof(RectTransform));
			gameObject.transform.SetParent(transform);
			gameObject.transform.localScale = Vector3.one;
			gameObject.AddComponent<RectTransform>();

			var image = gameObject.AddComponent<Image>();
			image.sprite = GetSprite(character);

			characterInstances.Add(gameObject);
		}
	}

	private void RePosition()
	{
		var parentRectTransform = GetComponent<RectTransform>();

		for (int i=0; i < text.Length; i++)
		{
			var rectTransform = characterInstances[i].GetComponent<RectTransform>();
			rectTransform.anchoredPosition = parentRectTransform.anchoredPosition;

			var relativePosition = MakeRelativePosition(text.Length, i, align);

			rectTransform.offsetMax = new Vector2(100, 100) + relativePosition;
			rectTransform.offsetMin = Vector2.zero + relativePosition;
			rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, rectTransform.localPosition.y, 0);
		}
	}

	private static Vector2 MakeRelativePosition(int totalCount, int index, Align align)
	{
		switch (align) {
			case Align.LEFT:
				return new Vector2(gap * index, 0);
			case Align.RIGHT:
				var reverseIndex = index - (totalCount - 1);
				return new Vector2(gap * reverseIndex, 0);
			case Align.MIDDLE:
				float middleIndex = (totalCount - 1.0f) / 2.0f;
				return new Vector2(gap * (index - middleIndex), 0);
			default:
				return Vector2.zero;
		}
	}

	private Sprite GetSprite(char character)
	{
		switch (character)
		{
			case '0':
			return sprite0;
			case '1':
			return sprite1;
			case '2':
			return sprite2;
			case '3':
			return sprite3;
			case '4':
			return sprite4;
			case '5':
			return sprite5;
			case '6':
			return sprite6;
			case '7':
			return sprite7;
			case '8':
			return sprite8;
			case '9':
			return sprite9;
			case '-':
			return spriteMinus;
			case '+':
			return spritePlus;
		}
		throw new System.Exception("Cannot find font of " + character);
	}
}
