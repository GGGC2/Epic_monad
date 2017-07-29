using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChainList : MonoBehaviour {
	// 체인리스트 관련 함수.

	// *주의 : SkillAndChainStates.cs에서 같은 이름의 함수를 수정할 것!!
	public static List<Tile> GetRouteTiles(List<Tile> tiles)
	{
		List<Tile> newRouteTiles = new List<Tile>();
		foreach (var tile in tiles)
		{
			// 타일 단차에 의한 부분(미구현)
			// 즉시 탐색을 종료한다.
			// break;
			
			// 첫 유닛을 만난 경우
			// 이번 타일을 마지막으로 종료한다.
			newRouteTiles.Add(tile);
			if (tile.IsUnitOnTile())
				break;
		}

		return newRouteTiles;
	}

	// 체인리스트에서 경로형 기술 범위 재설정. 이동 후, 스킬시전 후(유닛 사망, 넉백, 풀링 등) 꼭 호출해줄 것
	public static List<ChainInfo> RefreshChainInfo(List<ChainInfo> chainList)
	{
		TileManager tileManager = FindObjectOfType<TileManager>();

		List<ChainInfo> newChainList = new List<ChainInfo>();
		foreach (var chainInfo in chainList)
		{
			if (chainInfo.IsRouteType())
			{
				List<Tile> newRouteTiles = Battle.Turn.SkillAndChainStates.GetRouteTiles(chainInfo.GetRouteArea());
				Tile newCenterTile = newRouteTiles[newRouteTiles.Count-1];
				ActiveSkill skill = chainInfo.GetSkill();
				Unit unit = chainInfo.GetUnit();
				List<Tile> newTargetTiles = tileManager.GetTilesInRange(skill.GetSecondRangeForm(), 
																	newCenterTile.GetTilePos(),
																	skill.GetSecondMinReach(),
																	skill.GetSecondMaxReach(),
																	skill.GetSecondWidth(),
																	unit.GetDirection());

				ChainInfo newChainInfo = new ChainInfo(unit, newCenterTile, newTargetTiles, skill, chainInfo.GetRouteArea());
				newChainList.Add(newChainInfo);
			}
			else
				newChainList.Add(chainInfo);
		}

		return newChainList;
	}

	public static void AddChains(Unit unit, Tile targetTile, List<Tile> targetArea, ActiveSkill skill, List<Tile> firstRange)
	{
		List<ChainInfo> chainList = FindObjectOfType<BattleManager>().GetChainList();

		ChainInfo newChainInfo = new ChainInfo(unit, targetTile, targetArea, skill, firstRange);
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
