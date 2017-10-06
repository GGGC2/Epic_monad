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
        public override float GetStatusEffectVar(UnitStatusEffect statusEffect, int i, Unit caster, Unit owner) {
            return caster.StatusEffectList.Count(x => x.GetIsBuff() && (x.GetCaster() != caster));
        }
    }
}
