using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

public class ChainList : MonoBehaviour {
	private static List<Chain> chainList;
	public static void InitiateChainList(){
		chainList = new List<Chain> ();
	}

	public static void AddChains(Casting casting)
	{
		Chain newChain = new Chain(casting);
		chainList.Add(newChain);
		SetChargeEffectToUnit(casting.Caster);
	}
	private static void SetChargeEffectToUnit(Unit unit)
	{
		GameObject effect = Instantiate(Resources.Load("Effect/Waiting")) as GameObject;
		unit.SetChargeEffect(effect);
	}

	public static void RemoveChainOfThisUnit(Unit unit)
	{
		Chain myChain = chainList.Find(x => x.Caster == unit);
		chainList.Remove(myChain);
		RemoveChargeEffectOfUnit(unit);
	}
	private static void RemoveChargeEffectOfUnit(Unit unit)
	{
		unit.RemoveChargeEffect();
	}

	public static void ShowChainOfThisUnit(Unit unit)
	{
		Chain chain = GetChainOfThisUnit(unit);
		if (chain == null)
		{
			return;
		}
		List<Tile> targetArea = chain.SecondRange;
		TileManager.Instance.PaintTiles(targetArea, TileColor.Yellow);
	}
	public static void HideChainYellowDisplay()
	{
		TileManager.Instance.DepaintAllTiles (TileColor.Yellow);
	}
	private static Chain GetChainOfThisUnit(Unit unit)
	{
		foreach (Chain chain in chainList)
		{
			if (chain.Caster == unit)
			{
				return chain;
			}
		}
		return null;
	}

	public static void ShowUnitsTargetingThisTile(Tile tile)
	{
		List<Unit> units = GetUnitsTargetingThisTile (tile);
		foreach (Unit unit in units)
		{
			unit.ShowChainIcon ();
		}
	}
	public static void HideUnitsTargetingThisTile(Tile tile)
	{
		List<Unit> units = GetUnitsTargetingThisTile (tile);
		foreach (Unit unit in units)
		{
			unit.HideChainIcon ();
		}
	}
	private static List<Unit> GetUnitsTargetingThisTile(Tile tile)
	{
		List<Unit> resultUnits = new List<Unit>();
		foreach (Chain chain in chainList)
		{
			if (chain.RealEffectRange.Contains(tile))
			{
				resultUnits.Add(chain.Caster);
			}
		}
		return resultUnits;
	}

	// 현재 기술 시전을 첫 원소로 넣은 후, 해당 시전 후 발동되는 모든 체인 추가
	public static List<Chain> GetAllChainTriggered(Casting casting)
	{
		Unit caster = casting.Caster;
		Chain nowChain = new Chain (casting);
		List<Chain> allTriggeredChains = new List<Chain>();
		allTriggeredChains.Add (nowChain);

		//체인 발동계열 스킬(공격/약화)이어야 다른 체인을 발동시킨다.
		//아니라면 그냥 현재 시전만 넣은 리스트를 반환하게 됨
		if (casting.Skill.IsChainable ())
		{
			foreach (var chain in chainList)
			{
				// 같은 진영이 대기한 체인 중 공격범위 안의 유닛이 서로 겹치면 추가
				if (chain.Caster.IsAlly (caster) && chain.Overlapped (nowChain))
				{
					allTriggeredChains.Add (chain);
				}
			}
		}
		return allTriggeredChains;
	}
}
