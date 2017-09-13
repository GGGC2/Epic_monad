using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Battle.Damage;

namespace Battle.Skills
{
    public class Reina_2_l_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerActiveSkillDamageApplied(Unit caster, Unit target)
        {
            StatusEffector.AttachStatusEffect(caster, this.passiveSkill, target);
        }
        public override bool TriggerStatusEffectAppliedToOwner(UnitStatusEffect statusEffect, Unit caster, Unit target) {
            int numberOfBuffsFromOthers = caster.StatusEffectList.Count(x => x.GetIsBuff() && (x.GetCaster() != caster));
            statusEffect.CalculateAmount(0, numberOfBuffsFromOthers);
            return true;
        }
    }
}
