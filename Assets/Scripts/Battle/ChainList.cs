using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainList : MonoBehaviour {
	// 체인리스트 관련 함수.

	public static void AddChains(Casting casting)
	{
		List<Chain> chainList = FindObjectOfType<BattleManager>().GetChainList();

		Chain newChain = new Chain(casting);
		chainList.Add(newChain);

		SetChargeEffectToUnit(casting.Caster);
	}

	static void SetChargeEffectToUnit(Unit unit)
	{
		GameObject effect = Instantiate(Resources.Load("Effect/Waiting")) as GameObject;
		unit.SetChargeEffect(effect);
	}

	// 자신이 건 체인 삭제.
	public static void RemoveChainsFromUnit(Unit unit)
	{
		List<Chain> chainList = FindObjectOfType<BattleManager>().GetChainList();

		Chain deleteChain = chainList.Find(x => x.Caster == unit);

		chainList.Remove(deleteChain);

		RemoveChargeEffectToUnit(unit);
	}

	static void RemoveChargeEffectToUnit(Unit unit)
	{
		unit.RemoveChargeEffect();
	}

	// 현재 기술 시전을 첫 원소로 넣은 후, 해당 시전 후 발동되는 모든 체인 추가
	public static List<Chain> GetAllChainTriggered(Casting casting)
	{
		Unit caster = casting.Caster;
		List<Chain> allChainTriggered = new List<Chain>();
		allChainTriggered.Add (new Chain (casting));

		//체인 발동계열 스킬(공격/약화)이어야 다른 체인을 발동시킨다.
		//아니라면 그냥 현재 시전만 넣은 리스트를 반환하게 됨
		if (casting.Skill.IsChainable ()) {
			List<Chain> chainList = FindObjectOfType<BattleManager> ().GetChainList ();
			foreach (var chain in chainList) {
				// 같은 진영이 대기한 체인 중 공격범위 안의 유닛이 서로 겹치면 추가
				if (chain.Caster.IsAlly (caster)
					&& chain.Overlapped (new Chain (casting))) {
					allChainTriggered.Add (chain);
				}
			}
		}
		return allChainTriggered;
	}
}
