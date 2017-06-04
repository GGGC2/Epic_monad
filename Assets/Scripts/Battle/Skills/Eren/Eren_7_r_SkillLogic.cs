using UnityEngine;
using System.Linq;
using Battle.Damage;
using System.Collections.Generic;

namespace Battle.Skills
{
    public class Eren_7_r_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnStart(Unit caster) {
            StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
        }
        public override float GetStatusEffectVar(StatusEffect statusEffect, int i, Unit eren) {
            StatusEffect AbsorptionStatusEffect = eren.GetStatusEffectList().Find(se => se.GetDisplayName() == "흡수");
            int stack = 0;
            if (AbsorptionStatusEffect != null)
                stack = AbsorptionStatusEffect.GetRemainStack();
            return stack;
        }
    }
}
