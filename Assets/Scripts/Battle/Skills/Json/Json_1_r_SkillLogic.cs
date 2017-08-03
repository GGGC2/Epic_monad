using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Battle.Damage;

namespace Battle.Skills {
    class Json_1_r_SkillLogic : BaseSkillLogic {
        public override IEnumerator ActionInDamageRoutine(SkillInstanceData skillInstanceData) {
            Unit caster = skillInstanceData.GetCaster();
            Unit target = skillInstanceData.GetMainTarget();
            PassiveSkill mark = caster.GetLearnedPassiveSkillList().Find(skill => skill.GetName() == "표식");
            List<StatusEffect.FixedElement> fixedStatusEffects = mark.GetStatusEffectList();
            List<StatusEffect> statusEffects = fixedStatusEffects
                .Select(fixedElem => new StatusEffect(fixedElem, caster, target, null, mark))
                .ToList();
            StatusEffector.AttachStatusEffect(caster, statusEffects, target);
            yield return null;
        }
    }
}
