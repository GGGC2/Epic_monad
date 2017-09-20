using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomUIText : MonoBehaviour
{
	public string text = "013";
	public Align align;

	public Image image0;
	public Image image1;
	public Image image2;
	public Image image3;
	public Image image4;
	public Image image5;
	public Image image6;
	public Image image7;
	public Image image8;
	public Image image9;

	private List<GameObject> characterInstances = new List<GameObject>();

	public enum Align
	{
		LEFT,
		MIDDLE,
		RIGHT
	}

	void Start(){
		DestroyAllChilds();
		GenerateTextInstances();
		RePosition();

		StartCoroutine(CheckValueChanged());
	}

	void OnEnable()
	{
		DestroyAllChilds();
		GenerateTextInstances();
		RePosition();
	}

	void OnDisable()
	{
		DestroyAllChilds();
	}

	public void RefreshOnInspector()
	{
		List<Transform> childs = new List<Transform>();
		foreach (Transform child in transform)
		{
			// We cannot destroy child when iterating transform
			childs.Add(child);
		}

		foreach (Transform child in childs)
		{
			// In Editor we cannot use Destroy function
			DestroyImmediate(child.gameObject);
		}

		characterInstances.Clear();
		GenerateTextInstances();
		RePosition();
	}

	IEnumerator CheckValueChanged(){
		var previousText = text;
		var previousAlign = align;

		while (true){
			yield return null;
			if (previousText != text || previousAlign != align){
				previousText = text;
				previousAlign = align;

				DestroyAllChilds();
				GenerateTextInstances();
				RePosition();
			}
		}
	}

	private void DestroyAllChilds()
	{
		List<Transform> childs = new List<Transform>();
		foreach (Transform child in transform)
		{
			// Cannot remove child when iterating transform.
			childs.Add(child);
		}

		foreach (Transform child in childs)
		{
			Destroy(child.gameObject);
		}
		characterInstances.Clear();
	}

	private void GenerateTextInstances()
	{
		foreach (var character in text)
		{
			Image image = Instantiate(GetPrefab(character)) as Image;
			image.transform.SetParent(transform);

			characterInstances.Add(image.gameObject);
		}
	}

	private void RePosition()
	{
		var parentRectTransform = GetComponent<RectTransform>();

		float accumulatedWidth = 0;
		float totalWidth = 0;

		foreach (GameObject characterInstance in characterInstances)
		{
			RectTransform rectTransform = characterInstance.GetComponent<RectTransform>();
			totalWidth += rectTransform.rect.width;
		}

		for (int i=0; i < text.Length; i++)
		{
			var rectTransform = characterInstances[i].GetComponent<RectTransform>();
			rectTransform.anchoredPosition = parentRectTransform.anchoredPosition;
			rectTransform.localScale = Vector3.one;

			var relativePosition = MakeRelativePosition((int)accumulatedWidth, (int)totalWidth, align);
			rectTransform.anchoredPosition = relativePosition;

			accumulatedWidth += rectTransform.rect.width;
		}
	}

	private static Vector2 MakeRelativePosition(int accumulatedWidth, int totalWidth, Align align)
	{
		switch (align) {
			case Align.LEFT:
				return new Vector2(accumulatedWidth, 0);
			case Align.MIDDLE:
				return new Vector2(accumulatedWidth - (totalWidth / 2), 0);
			case Align.RIGHT:
				return new Vector2(accumulatedWidth - totalWidth, 0);
			default:
				Debug.LogWarning("Invalid align");
				return Vector2.zero;
		}
	}

	private Image GetPrefab(char character){
		switch (character){
			case '0':
				return image0;
			case '1':
				return image1;
			case '2':
				return image2;
			case '3':
				return image3;
			case '4':
				return image4;
			case '5':
				return image5;
			case '6':
				return image6;
			case '7':
				return image7;
			case '8':
				return image8;
			case '9':
				return image9;
			default:
				Debug.LogError("Cannot show character " + character);
				return image0;
		}
	}
}
