using Battle.Damage;
using System.Collections.Generic;

namespace Battle.Skills {
    class Eugene_3_r_SkillLogic : BasePassiveSkillLogic{
        public override void TriggerOnPhaseStart(Unit caster) {
            StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
        }
        public override void TriggerUsingSkill(Unit caster, List<Unit> targets) {
            StatusEffect statusEffectToRemove = caster.GetStatusEffectList().Find(se => se.GetOriginSkillName() == "여행자의 발걸음");
            caster.RemoveStatusEffect(statusEffectToRemove);
        }
    }
}
