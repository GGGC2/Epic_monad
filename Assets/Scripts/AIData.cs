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
	public List<int> activeTriggers = new List<int>();
	public bool isActive = false;

	// 활성화 페이즈 (2번 트리거에서 사용)
	public int activePhase = 2;

	// 활성범위 (3, 4번 트리거에서 사용)
	public List<List<Tile>> trigger3Area = new List<List<Tile>>();
	public List<List<Tile>> trigger4Area = new List<List<Tile>>();

	public void Awake()
	{
		// activeTriggers.Add(3);
		// activeTriggers.Add(5);
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
		if (activeTriggers.Contains(5))
			SetActive();
	}

	public void SetAIInfo(AIInfo info)
	{
		activeTriggers = info.activeTriggers;
		activePhase = info.activePhase;
		trigger3Area = info.trigger3Area;
		trigger4Area = info.trigger4Area;
	}
}
