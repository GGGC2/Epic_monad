using System.Collections.Generic;
using Battle.Damage;

namespace Battle.Skills {
    class Eugene_7_r_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnTurnStart(Unit caster, Unit turnStarter) {
            if(Utility.GetDistance(caster.GetPosition(), turnStarter.GetPosition()) <= 2) {
                UnitStatusEffect.FixedElement fixedElement = passiveSkill.GetUnitStatusEffectList().Find(se => se.display.displayName == "길잡이");
                UnitStatusEffect statusEffectToAttach = new UnitStatusEffect(fixedElement, caster, turnStarter, passiveSkill);
                turnStarter.GetStatusEffectList().Add(statusEffectToAttach);
            }
        }
        public override void TriggerStatusEffectsOnMove(Unit target, UnitStatusEffect statusEffect) {
            if (!target.GetHasUsedSkillThisTurn())
                StatusEffector.AttachStatusEffect(statusEffect.GetCaster(), passiveSkill, target);
        }
    }
}
