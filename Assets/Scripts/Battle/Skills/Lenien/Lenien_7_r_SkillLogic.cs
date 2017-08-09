using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Battle.Damage;

namespace Battle.Skills
{
    public class Lenien_7_r_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnStart(Unit caster) {
            StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
        }
        public override float GetStatusEffectVar(UnitStatusEffect statusEffect, int i, Unit caster, Unit owner) {
            TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();
            List<Tile> nearbyTilesOfLenian = tileManager.GetTilesInRange(Enums.RangeForm.Square, owner.GetPosition(), 0, 1, 0, owner.GetDirection());
            return nearbyTilesOfLenian.Count(x => x.GetTileElement() == Enums.Element.Metal);
        }
    }
}
