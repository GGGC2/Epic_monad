using System.Collections;
using System.Collections.Generic;
using Battle.Damage;
using Enums;
using UnityEngine;

namespace Battle.Skills {
    class Json_0_1_SkillLogic : BasePassiveSkillLogic {
        public override IEnumerator ActionInDamageRoutine(CastingApply castingApply) {
            Unit caster = castingApply.GetCaster();
            Unit target = castingApply.GetTarget();

            UnitStatusEffect.FixedElement evasionFixedElem = passiveSkill.GetUnitStatusEffectList().Find(se => se.display.displayName == "회피");
            UnitStatusEffect evasion = new UnitStatusEffect(evasionFixedElem, caster, caster, passiveSkill);
            List<UnitStatusEffect> evasionStatusEffects = new List<UnitStatusEffect>();
            evasionStatusEffects.Add(evasion);

            UnitStatusEffect.FixedElement markFixedElem = passiveSkill.GetUnitStatusEffectList().Find(se => se.display.displayName == "표식");
            UnitStatusEffect mark = new UnitStatusEffect(markFixedElem, caster, target, passiveSkill);
            List<UnitStatusEffect> markStatusEffects = new List<UnitStatusEffect>();
            markStatusEffects.Add(mark);

            UnitStatusEffect alreadyAppliedStatusEffect = target.StatusEffectList.Find(se => se.GetDisplayName() == "표식");
            if(alreadyAppliedStatusEffect != null && alreadyAppliedStatusEffect.GetRemainStack() >= 4) {
                StatusEffector.AttachStatusEffect(caster, evasionStatusEffects, caster);
                target.RemoveStatusEffect(alreadyAppliedStatusEffect);
            }
            else StatusEffector.AttachStatusEffect(caster, markStatusEffects, target);
            yield return null;
        }
    }
}
