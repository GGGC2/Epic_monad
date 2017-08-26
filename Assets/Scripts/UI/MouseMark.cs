using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseMark : MonoBehaviour{
	Image image;
	IEnumerator Start(){
		image = GetComponent<Image>();
		float timeInterval = 0.5f;
		int repeatTime = 2;
		for(int i = 0; i < repeatTime; i++){
			yield return new WaitForSeconds(timeInterval);
			image.enabled = false;
			yield return new WaitForSeconds(timeInterval);
			image.enabled = true;
		}
		yield return new WaitForSeconds(timeInterval);
		Destroy(gameObject);
	}
}