using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChainList : MonoBehaviour {
	// 체인리스트 관련 함수.

	public static void AddChains(Unit unit, ActiveSkill skill, SkillLocation skillLocation)
	{
		List<ChainInfo> chainList = FindObjectOfType<BattleManager>().GetChainList();

		ChainInfo newChainInfo = new ChainInfo(unit, skill, skillLocation);
		chainList.Add(newChainInfo);

		SetChargeEffectToUnit(unit);
	}

	static void SetChargeEffectToUnit(Unit unit)
	{
		GameObject effect = Instantiate(Resources.Load("Effect/Waiting")) as GameObject;
		unit.SetChargeEffect(effect);
	}

	// 자신이 건 체인 삭제.
	public static void RemoveChainsFromUnit(Unit unit)
	{
		List<ChainInfo> chainList = FindObjectOfType<BattleManager>().GetChainList();

		ChainInfo deleteChainInfo = chainList.Find(x => x.GetUnit() == unit);
        
	    chainList.Remove(deleteChainInfo);

		RemoveChargeEffectToUnit(unit);
	}

	static void RemoveChargeEffectToUnit(Unit unit)
	{
		unit.RemoveChargeEffect();
	}

	// 해당 영역에 체인을 대기중인 모든 정보 추출 (같은 진영만)
	public static List<ChainInfo> GetAllChainInfoToTargetArea(Unit unit, List<Tile> targetArea)
	{
		List<ChainInfo> chainList = FindObjectOfType<BattleManager>().GetChainList();

		List<ChainInfo> allChainInfoToTargetArea = new List<ChainInfo>();
		foreach (var chainInfo in chainList)
		{
			// 공격범위 안의 유닛이 서로 겹치거나, 체인을 건 본인일 경우 추가.
			if (((unit.GetSide() == chainInfo.GetUnit().GetSide())
				 && (chainInfo.Overlapped(targetArea)))
				 || (chainInfo.GetUnit() == unit))
			{
				allChainInfoToTargetArea.Add(chainInfo);
			}
		}

		return allChainInfoToTargetArea;
	}

	// 서로 다른 모든 체인 유닛 추출
	public static List<Unit> GetAllUnitsInChainList(List<ChainInfo> chainInfoList)
	{
		List<ChainInfo> chainList = FindObjectOfType<BattleManager>().GetChainList();

		List<Unit> units = new List<Unit>();
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
