using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;
using System.Linq;
using Battle.Damage;

namespace Battle.Skills
{
public class Eren_7_l_SkillLogic : BasePassiveSkillLogic {
	public override void ApplyStatusEffectByKill(HitInfo hitInfo, Unit deadUnit)
	{
		TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();
		Vector2 deadUnitPos = deadUnit.GetPosition();

		List<Tile> applyArea = tileManager.GetTilesInRange(RangeForm.Diamond, deadUnitPos, 1, 2, Direction.LeftDown);
		List<Unit> targets = new List<Unit>();
		foreach (var tile in applyArea)
		{
			if (tile.IsUnitOnTile())
			{
				Unit target = tile.GetUnitOnTile();
				if (target.GetSide() == Side.Enemy)
				{
					targets.Add(target);
				}
			}
		}

		foreach (var target in targets)
			StatusEffector.AttachStatusEffect(hitInfo.caster, this.passiveSkill, target);
	}

	public override void SetAmountToEachStatusEffect(List<StatusEffect> statusEffects, Unit caster, Unit target)
	{
		statusEffects[0].SetRemainStack(1);
		statusEffects[0].SetRemainPhase(999);
		statusEffects[0].SetAmount(10);
	}
}
}