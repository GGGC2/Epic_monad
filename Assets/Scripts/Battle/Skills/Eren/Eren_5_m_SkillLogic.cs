using UnityEngine;
using System.Linq;
using Battle.Damage;
using System.Collections.Generic;

namespace Battle.Skills
{
    public class Eren_5_m_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnStart(Unit caster) {
            StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
        }
        public override float GetStatusEffectVar(UnitStatusEffect statusEffect, int i, Unit caster, Unit owner) {
            UnitStatusEffect AbsorptionStatusEffect = owner.GetStatusEffectList().Find(se => se.GetDisplayName() == "흡수");
            int stack = 0;
            if (AbsorptionStatusEffect != null)
                stack = AbsorptionStatusEffect.GetRemainStack();
            return stack;
        }
    }
}
