using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

public class AIData : MonoBehaviour {

	// 활성화 트리거
	// 1: 전투 시작하면 바로
	// 2: 일정 페이즈가 되면
	// 3: 자신 주위 일정 영역에 동료가 아닌 유닛이 들어오면
	// 4: 맵 상의 일정 영역에 동료가 아닌 유닛이 들어오면
	// 5: 자신이 스킬의 대상이 되거나 다른 이유로 데미지/치유/효과를 받으면
	// 6: 활성화되지 않음
	public List<int> activeTrigger = new List<int>();
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

	public void Awake()
	{
		activeTrigger.Add(3);
		activeTrigger.Add(5);
	}

	public bool IsActive()
	{
		return isActive;
	}

	public void SetActive()
	{
		isActive = true;
	}

	public void SetActiveByExternalFactor()
	{
		if (activeTrigger.Contains(5))
			SetActive();
	}
}
