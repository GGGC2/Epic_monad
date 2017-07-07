using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Battle.Damage;

namespace Battle.Skills
{
    public class Reina_7_m_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerActiveSkillDamageApplied(Unit caster, Unit target) {
            StatusEffector.AttachStatusEffect(caster, this.passiveSkill, target);
        }
        public override bool TriggerStatusEffectAppliedToOwner(StatusEffect statusEffect, Unit caster, Unit target) {
            int numberOfBuffsFromOthers = caster.GetStatusEffectList().Count(x => x.GetIsBuff() && (x.GetCaster() != caster));
            statusEffect.CalculateAmount(numberOfBuffsFromOthers);
            return true;
        }
    }
}
