using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Battle.Damage;
using Enums;
using Battle.Skills;

namespace Battle.Skills {
    class Eugene_7_r_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnTurnStart(Unit caster, Unit turnStarter) {
            if(Utility.GetDistance(caster.GetPosition(), turnStarter.GetPosition()) <= 2) {
                StatusEffector.AttachStatusEffect(caster, passiveSkill, turnStarter);
            }
        }
        public override void TriggerStatusEffectsOnUsingSkill(Unit target, List<Unit> targetsOfSkill, StatusEffect stautsEffect) {
            StatusEffect statusEffectToRemove = target.GetStatusEffectList().Find(se => se.GetOriginSkillName() == "길잡이");
            if (statusEffectToRemove != null)
                target.RemoveStatusEffect(statusEffectToRemove);
        }
    }
}
