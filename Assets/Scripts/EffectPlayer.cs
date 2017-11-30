using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Battle;
using Battle.Skills;
using Enums;
using DG.Tweening;
using System.Linq;

public static class EffectPlayer {

	public static void ApplySoundEffect(ActiveSkill skill){
		string soundEffectName = skill.soundEffectName;
        if (soundEffectName != null && soundEffectName != "-") {
            SoundManager.Instance.PlaySE(soundEffectName);
        }
	}

	static Vector3 GetCenterPosOfTiles(List<Tile> tiles) {
		Vector3 centerPos = Vector3.zero;
		foreach (var tile in tiles) {
				centerPos += tile.realPosition;
		}
		centerPos = centerPos / (float)tiles.Count;
		return centerPos;
	}

	public static IEnumerator ApplyVisualEffect(Casting casting) {
        Unit unit = casting.Caster;
        List<Tile> secondRange = casting.SecondRange;
		string visualEffectName = casting.Skill.visualEffectName;
		EffectVisualType effectVisualType = casting.Skill.effectVisualType;
		EffectMoveType effectMoveType = casting.Skill.effectMoveType;
		bool effectDirectionality = casting.Skill.effectDirectionality;

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

		// 범위형 이펙트
		if (effectVisualType == EffectVisualType.Area) {

			Vector3 startPos;
			Vector3 endPos;

			if (effectMoveType == EffectMoveType.Move) {
				// 이동형, 범위형 이펙트.
				startPos = unit.realPosition + Vector3.back * 0.01f;
				// endPos = GetCenterPosOfTiles(secondRange) + Vector3.back * 0.1f;
				endPos = (Vector3)casting.PivotPos + Vector3.back * 0.1f;
			} 
			else /* if (effectVisualType == EffectVisualType.Area) */{
				// 고정형, 범위형 이펙트.
				// startPos = GetCenterPosOfTiles(secondRange) + Vector3.back * 0.1f;
				startPos = (Vector3)casting.PivotPos + Vector3.back * 5f;
				endPos = startPos;
			}

			GameObject particlePrefab = Resources.Load("Particle/" + visualEffectName) as GameObject;
			if (particlePrefab == null) {
				Debug.LogError("Cannot load particle " + visualEffectName);
			}
			GameObject particle = GameObject.Instantiate(particlePrefab) as GameObject;			

			// 방향 설정 (방향성 가진 이펙트 한정)
			if (effectDirectionality) {
				List<ParticleSystem> subParticles = particle.GetComponentsInChildren<ParticleSystem>().ToList();
				if (casting.Location.Direction == Direction.LeftUp) {
					subParticles.ForEach(p => {
						var main = p.main;
						main.startRotationZ = 180f * 180f / 10313.24f;
					});
				}
				else if (casting.Location.Direction == Direction.LeftDown) {
					subParticles.ForEach(p => {
						var main = p.main;
						main.startRotationY = 180f * 180f / 10313.24f;
					});
				}
				else if (casting.Location.Direction == Direction.RightUp) {
					subParticles.ForEach(p => {
						var main = p.main;
						main.startRotationX = 180f * 180f / 10313.24f;
					});
				}
				else /*(casting.Location.Direction == Direction.RightDown)*/ {
					// subParticles.ForEach(p => p.startRotation3D = new Vector3(0,0,0));
				}
			}

			particle.transform.position = startPos; Debug.Log(particle.transform.position);
			yield return new WaitForSeconds (0.4f * EFFECTTIME);
			Tween tw = particle.transform.DOMove(endPos, 0.6f * EFFECTTIME);
			yield return tw.WaitForCompletion();
			GameObject.Destroy(particle, EFFECTTIME);
		} 
		else if (effectVisualType == EffectVisualType.Individual) {
			// 개별 대상 이펙트.
			Vector3 startPos = unit.realPosition + Vector3.back * 0.01f;
			List<Vector3> endPosList = new List<Vector3>();
			foreach (var tileObject in secondRange) {
				Tile tile = tileObject;
				if (tile.IsUnitOnTile()) {
					endPosList.Add(tile.GetUnitOnTile().realPosition + Vector3.back * 0.1f);
				}
			}

			foreach (var endPos in endPosList) {
				GameObject particle = GameObject.Instantiate(Resources.Load("Particle/" + visualEffectName)) as GameObject;

				// 방향 설정
				if (effectDirectionality) {
					List<ParticleSystem> subParticles = particle.GetComponentsInChildren<ParticleSystem>().ToList();
					if (casting.Location.Direction == Direction.LeftUp) {
					subParticles.ForEach(p => {
						var main = p.main;
						main.startRotationZ = 180f * 180f / 10313.24f;
					});
					}
					else if (casting.Location.Direction == Direction.LeftDown) {
						subParticles.ForEach(p => {
							var main = p.main;
							main.startRotationY = 180f * 180f / 10313.24f;
						});
					}
					else if (casting.Location.Direction == Direction.RightUp) {
						subParticles.ForEach(p => {
							var main = p.main;
							main.startRotationX = 180f * 180f / 10313.24f;
						});
					}
					else /*(casting.Location.Direction == Direction.RightDown)*/ {
						// subParticles.ForEach(p => p.startRotation3D = new Vector3(0,0,0));
					}
				}

				if (effectMoveType == EffectMoveType.Move) {
					// 이동형 이펙트.
					particle.transform.position = startPos;
				}
				else /* if (effectVisualType == EffectVisualType.Area) */{
					// 고정형 이펙트.
					particle.transform.position = endPos;
				}	
				Tween tw = particle.transform.DOMove(endPos, 0.6f * EFFECTTIME);
				GameObject.Destroy(particle, EFFECTTIME + 0.6f * EFFECTTIME); // 아랫줄에서의 지연시간을 고려한 값이어야 함.
			}

			// 대상이 없을 경우 이펙트 출력 안함.
			
			yield return new WaitForSeconds(EFFECTTIME);
        }
    }
}
