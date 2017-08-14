using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour {
	public Sprite dotCellSprite;
	Vector2 size;
	RectTransform rect;
	Image image;
	void Awake(){
		rect = GetComponent<RectTransform> ();
		image = GetComponent<Image> ();
	}
	public void SetSize(Vector2 size){
		this.size = size;
		rect.sizeDelta = this.size;
	}
	public void SetPosition(Vector2 pos, Vector2 offset){
		Vector2 realPos = new Vector2 (size.x * pos.x, size.y * pos.y) + offset;
		transform.localPosition = realPos;
	}
	public void SetColor(Color color){
		image.color = color;
	}
	public void SetAsDotCell(){
		image.sprite = dotCellSprite;
	}
}
