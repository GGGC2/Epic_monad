using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;
using System.Linq;
using Battle.Damage;

namespace Battle.Skills
{
    public class Eren_7_l_SkillLogic : BasePassiveSkillLogic {
	    public override void TriggerOnKill(HitInfo hitInfo, Unit deadUnit)
	    {
		    TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();
		    Vector2 deadUnitPos = deadUnit.GetPosition();

		    List<Tile> applyArea = tileManager.GetTilesInRange(RangeForm.Diamond, deadUnitPos, 1, 2, 0, Direction.LeftDown);
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
    }
}