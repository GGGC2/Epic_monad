using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomUIText : MonoBehaviour{
	public string text = "";
	public Align align;
	public Image[] numberImages = new Image[10];

	private List<GameObject> characterInstances = new List<GameObject>();

	public enum Align{
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

	void OnEnable(){
		DestroyAllChilds();
		GenerateTextInstances();
		RePosition();
	}

	void OnDisable(){
		DestroyAllChilds();
	}

	public void RefreshOnInspector(){
		List<Transform> childs = new List<Transform>();
		foreach (Transform child in transform){
			// We cannot destroy child when iterating transform
			childs.Add(child);
		}

		foreach (Transform child in childs){
			// In Editor we cannot use Destroy function
			DestroyImmediate(child.gameObject);
		}
		characterInstances.Clear();
		//여기까지 모든 child를 삭제

		//여기부터 새로운 문자 instance를 생성한 후 위치 조절
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

	private void DestroyAllChilds(){
		List<Transform> childs = new List<Transform>();
		foreach (Transform child in transform){
			// Cannot remove child when iterating transform.
			childs.Add(child);
		}

		foreach (Transform child in childs)
		{
			Destroy(child.gameObject);
		}
		characterInstances.Clear();
	}

	private void GenerateTextInstances(){
		foreach (var character in text){
			Image image = Instantiate(GetPrefab(character)) as Image;
			image.transform.SetParent(transform);

			characterInstances.Add(image.gameObject);
		}
	}

	private void RePosition(){
		var parentRectTransform = GetComponent<RectTransform>();

		float accumulatedWidth = 0;
		float totalWidth = 0;

		foreach (GameObject characterInstance in characterInstances){
			RectTransform rectTransform = characterInstance.GetComponent<RectTransform>();
			totalWidth += rectTransform.rect.width;
		}

		for (int i = 0; i < text.Length; i++){
			var rectTransform = characterInstances[i].GetComponent<RectTransform>();
			rectTransform.anchoredPosition = parentRectTransform.anchoredPosition;
			rectTransform.localScale = Vector3.one;

			var relativePosition = MakeRelativePosition((int)accumulatedWidth, (int)totalWidth, align);
			rectTransform.anchoredPosition = relativePosition;

			accumulatedWidth += rectTransform.rect.width;
		}
	}

	private static Vector2 MakeRelativePosition(int accumulatedWidth, int totalWidth, Align align){
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
		int parsedValue = Int32.Parse(character.ToString());
		Debug.Assert(parsedValue >= 0 && parsedValue <= 9, character + " is not Valid input.");
		return numberImages[parsedValue];
	}
}