using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChainList : MonoBehaviour {
	// 체인리스트 관련 함수.

	// 새로운 체인 정보 추가. 들어가는 정보는 시전자, 적용범위, 스킬인덱스.
	public static void AddChains(GameObject unit, Tile targetTile, List<GameObject> targetArea, int skillIndex)
	{
		List<ChainInfo> chainList = FindObjectOfType<BattleManager>().GetChainList();

		ChainInfo newChainInfo = new ChainInfo(unit, targetTile, targetArea, skillIndex);
		chainList.Add(newChainInfo);

		SetChargeEffectToUnit(unit);
	}

	// 경로형 기술일 경우 이쪽으로. 경로형 범위(routeArea) 추가.
	public static void AddChains(GameObject unit, Tile targetTile, List<GameObject> targetArea, int skillIndex, List<GameObject> routeArea)
	{
		List<ChainInfo> chainList = FindObjectOfType<BattleManager>().GetChainList();

		ChainInfo newChainInfo = new ChainInfo(unit, targetTile, targetArea, skillIndex, routeArea);
		chainList.Add(newChainInfo);

		SetChargeEffectToUnit(unit);
	}

	static void SetChargeEffectToUnit(GameObject unit)
	{
		GameObject effect = Instantiate(Resources.Load("Effect/Waiting")) as GameObject;
		unit.GetComponent<Unit>().SetChargeEffect(effect);
	}

	// 자신이 건 체인 삭제.
	public static void RemoveChainsFromUnit(GameObject unit)
	{
		List<ChainInfo> chainList = FindObjectOfType<BattleManager>().GetChainList();

		ChainInfo deleteChainInfo = chainList.Find(x => x.GetUnit() == unit);

		if (deleteChainInfo == null)
			Debug.LogWarning(unit.GetComponent<Unit>().GetName() + "'s chainInfo is null");
		else
			chainList.Remove(deleteChainInfo);

		RemoveChargeEffectToUnit(unit);
	}

	static void RemoveChargeEffectToUnit(GameObject unit)
	{
		unit.GetComponent<Unit>().RemoveChargeEffect();
	}

	// 해당 영역에 체인을 대기중인 모든 정보 추출 (같은 진영만)
	public static List<ChainInfo> GetAllChainInfoToTargetArea(GameObject unit, List<GameObject> targetArea)
	{
		List<ChainInfo> chainList = FindObjectOfType<BattleManager>().GetChainList();

		List<ChainInfo> allChainInfoToTargetArea = new List<ChainInfo>();
		foreach (var chainInfo in chainList)
		{
			// 공격범위 안의 유닛이 서로 겹치거나, 체인을 건 본인일 경우 추가.
			if (((unit.GetComponent<Unit>().GetSide() == chainInfo.GetUnit().GetComponent<Unit>().GetSide())
				 && (chainInfo.Overlapped(targetArea)))
				 || (chainInfo.GetUnit() == unit))
			{
				allChainInfoToTargetArea.Add(chainInfo);
			}
		}

		return allChainInfoToTargetArea;
	}

	// 서로 다른 모든 체인 유닛 추출
	public static List<GameObject> GetAllUnitsInChainList(List<ChainInfo> chainInfoList)
	{
		List<ChainInfo> chainList = FindObjectOfType<BattleManager>().GetChainList();

		List<GameObject> units = new List<GameObject>();
		foreach (var chainInfo in chainInfoList)
		{
			if (!units.Contains(chainInfo.GetUnit()));
			{
				units.Add(chainInfo.GetUnit());
			}
		}
		return units;
	}
}
