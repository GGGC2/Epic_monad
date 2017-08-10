using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour {
	Vector2 size;
	RectTransform rect;
	SpriteRenderer spriteRenderer;
	void Awake(){
		rect = GetComponent<RectTransform> ();
		spriteRenderer = GetComponent<SpriteRenderer> ();
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
		spriteRenderer.color = color;
	}
}
