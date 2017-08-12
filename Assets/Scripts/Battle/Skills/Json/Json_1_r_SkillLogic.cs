using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Battle.Damage;

namespace Battle.Skills {
    class Json_1_r_SkillLogic : BaseSkillLogic {
        public override IEnumerator ActionInDamageRoutine(SkillInstanceData skillInstanceData) {
            Unit caster = skillInstanceData.GetCaster();
            Unit target = skillInstanceData.GetTarget();
            PassiveSkill mark = caster.GetLearnedPassiveSkillList().Find(skill => skill.GetName() == "표식");
            List<UnitStatusEffect.FixedElement> fixedStatusEffects = mark.GetUnitStatusEffectList();
            List<UnitStatusEffect> statusEffects = fixedStatusEffects
                .Select(fixedElem => new UnitStatusEffect(fixedElem, caster, target, mark))
                .ToList();
            StatusEffector.AttachStatusEffect(caster, statusEffects, target);
            yield return null;
        }
    }
}
