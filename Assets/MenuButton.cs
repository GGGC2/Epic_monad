﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButton : MonoBehaviour{
	//유니티 버튼 함수는 parameter를 하나밖에 줄 수 없어서 함수를 쪼개놓음
	public void ActivateObject(GameObject TargetObject){
		TargetObject.SetActive(true);
	}

	public void DeactivateObject(GameObject TargetObject){
		TargetObject.SetActive(false);
	}
}