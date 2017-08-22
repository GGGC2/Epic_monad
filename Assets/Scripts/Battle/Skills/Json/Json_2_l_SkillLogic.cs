using Battle.Damage;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Skills {
    class Json_2_l_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerAfterMove(Unit caster, Tile beforeTile, Tile afterTile) {
            int dist = Utility.GetDistance(beforeTile.position, afterTile.position);
            UnitStatusEffect.FixedElement fixedElem = passiveSkill.GetUnitStatusEffectList()[0];
            UnitStatusEffect statusEffectToAttach = new UnitStatusEffect(fixedElem, caster, caster, passiveSkill);
            statusEffectToAttach.CalculateAmount(0, dist);
            List<UnitStatusEffect> listStatusEffect = new List<UnitStatusEffect>();
            listStatusEffect.Add(statusEffectToAttach);

            StatusEffector.AttachStatusEffect(caster, listStatusEffect, caster);
        }
    }
}
