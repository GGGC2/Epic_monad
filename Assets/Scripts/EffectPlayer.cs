using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battle;
using Battle.Skills;
using Enums;

public static class EffectPlayer {

	public static void ApplySoundEffect(ActiveSkill skill){
		string soundEffectName = skill.soundEffectName;
        if (soundEffectName != null && soundEffectName != "-") {
            SoundManager.Instance.PlaySE(soundEffectName);
        }
	}

	public static IEnumerator ApplyVisualEffect(Casting casting) {
        Unit unit = casting.Caster;
        List<Tile> secondRange = casting.SecondRange;
		string visualEffectName = casting.Skill.visualEffectName;
		EffectVisualType effectVisualType = casting.Skill.effectVisualType;
		EffectMoveType effectMoveType = casting.Skill.effectMoveType;

		if (secondRange.Count == 0) {
			yield break;
		}

		if (casting.Skill.visualEffectName == "-") {
			yield break;
		}

		float EFFECTTIME = 0.5f;
		if (BattleData.currentState == CurrentState.AITurn) {
			EFFECTTIME = 0.25f;
		}

		if ((effectVisualType == EffectVisualType.Area) && (effectMoveType == EffectMoveType.Move)) {
			// 투사체, 범위형 이펙트.
			Vector3 startPos = unit.realPosition;
			Vector3 endPos = new Vector3(0, 0, 0);
			foreach (var tile in secondRange) {
				endPos += tile.realPosition;
			}
			endPos = endPos / (float)secondRange.Count;

			GameObject particle = GameObject.Instantiate(Resources.Load("Particle/" + visualEffectName)) as GameObject;
			particle.transform.position = startPos - new Vector3(0, -0.5f, 0.01f);
			yield return new WaitForSeconds (0.4f * EFFECTTIME);
			// 타일 축 -> 유닛 축으로 옮기기 위해 z축으로 5만큼 앞으로 빼준다.
			// 유닛의 중앙 부분을 공격하기 위하여 y축으로 0.5 올린다.
			iTween.MoveTo(particle, endPos - new Vector3(0, 0, 0.01f) - new Vector3(0, -0.5f, 5f), 0.5f);
			yield return new WaitForSeconds (0.6f * EFFECTTIME);
			GameObject.Destroy(particle, EFFECTTIME);
			yield return null;
		} else if ((effectVisualType == EffectVisualType.Area) && (effectMoveType == EffectMoveType.NonMove)) {
			// 고정형, 범위형 이펙트.
			Vector3 targetPos = new Vector3(0, 0, 0);
			foreach (var tile in secondRange) {
				targetPos += tile.realPosition;
			}
			targetPos = targetPos / (float)secondRange.Count;
			targetPos = targetPos - new Vector3(0, -0.5f, 5f); // 타일 축 -> 유닛 축으로 옮기기 위해 z축으로 5만큼 앞으로 빼준다.

			GameObject particlePrefab = Resources.Load("Particle/" + visualEffectName) as GameObject;
			if (particlePrefab == null) {
				Debug.LogError("Cannot load particle " + visualEffectName);
			}
			GameObject particle = GameObject.Instantiate(particlePrefab) as GameObject;
			particle.transform.position = targetPos - new Vector3(0, -0.5f, 0.01f);
			yield return new WaitForSeconds(EFFECTTIME);
			GameObject.Destroy(particle, EFFECTTIME);
			yield return null;
		} else if ((effectVisualType == EffectVisualType.Individual) && (effectMoveType == EffectMoveType.NonMove)) {
			// 고정형, 개별 대상 이펙트.
			List<Vector3> targetPosList = new List<Vector3>();
			foreach (var tileObject in secondRange) {
				Tile tile = tileObject;
				if (tile.IsUnitOnTile()) {
					targetPosList.Add(tile.GetUnitOnTile().realPosition);
				}
			}

			foreach (var targetPos in targetPosList) {
				GameObject particle = GameObject.Instantiate(Resources.Load("Particle/" + visualEffectName)) as GameObject;
				particle.transform.position = targetPos - new Vector3(0, -0.5f, 0.01f);
				GameObject.Destroy(particle, EFFECTTIME + 0.6f * EFFECTTIME); // 아랫줄에서의 지연시간을 고려한 값이어야 함.
			}

			if (targetPosList.Count == 0) // 대상이 없을 경우. 일단 가운데 이펙트를 띄운다.
			{
				Vector3 midPos = new Vector3(0, 0, 0);
				foreach (var tile in secondRange) {
					midPos += tile.realPosition;
				}
				midPos = midPos / (float)secondRange.Count;

				GameObject particle = GameObject.Instantiate(Resources.Load("Particle/" + visualEffectName)) as GameObject;
				particle.transform.position = midPos - new Vector3(0, -0.5f, 0.01f);
				GameObject.Destroy (particle, EFFECTTIME + 0.6f * EFFECTTIME); // 아랫줄에서의 지연시간을 고려한 값이어야 함.
			}
			yield return new WaitForSeconds(EFFECTTIME);
        }
    }
}
