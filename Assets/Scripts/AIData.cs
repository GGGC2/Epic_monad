using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

public class AIData : MonoBehaviour {

	// 활성화 트리거
	public int activeTrigger = 3;
	public bool isActive = false;

	// 활성화 페이즈 (2번 트리거에서 사용)
	public int activePhase = 2;

	// 활성범위 관련 변수 (3, 4번 트리거에서 사용)
	public RangeForm rangeForm = RangeForm.Diamond;
	// 트리거가 3일때는 (0,0)이 자신의 위치, 4일때는 절대좌표 (0,0)을 의미
	public Vector2 midPosition = new Vector2(0, 0);
	public int minReach = 1;
	public int maxReach = 5;
	public int width = 0;

	public bool IsActive()
	{
		return isActive;
	}

	public void SetActive()
	{
		isActive = true;
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
