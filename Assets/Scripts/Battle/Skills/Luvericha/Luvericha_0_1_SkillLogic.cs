using System.Collections.Generic;
using Enums;
using Battle.Damage;
using UnityEngine;

namespace Battle.Skills {
    class Luvericha_0_1_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerUsingSkill(Casting casting, List<Unit> targets) {
            Unit caster = casting.Caster;
            ActiveSkill skill = casting.Skill;
            SkillApplyType skillApplyType = skill.GetSkillApplyType();

            string displayName;
            string displayNameToRemove;
            List<UnitStatusEffect> statusEffectList = new List<UnitStatusEffect>();

            if (skillApplyType == SkillApplyType.DamageHealth || skillApplyType == SkillApplyType.DamageAP
                || skillApplyType == SkillApplyType.Debuff) {
                displayName = "포르테";
                displayNameToRemove = "피아노";
            } else {
                displayName = "피아노";
                displayNameToRemove = "포르테";
            }
            UnitStatusEffect statusEffectToRemove = caster.StatusEffectList.Find(se => se.GetDisplayName() == displayNameToRemove);
            if(statusEffectToRemove != null)
                caster.RemoveStatusEffect(statusEffectToRemove);

            UnitStatusEffect.FixedElement fixedElem = passiveSkill.GetUnitStatusEffectList().Find(se => se.display.displayName == displayName);
            UnitStatusEffect statusEffect = new UnitStatusEffect(fixedElem, caster, caster, passiveSkill);
            statusEffectList.Add(statusEffect);
            StatusEffector.AttachStatusEffect(caster, statusEffectList, caster);
        }

        public override void ApplyAdditionalDamageFromCasterStatusEffect(CastingApply castingApply, StatusEffect statusEffect) {
            SkillApplyType skillApplyType = castingApply.GetSkill().GetSkillApplyType();
            bool isStatusEffectForte = (statusEffect.GetDisplayName() == "포르테");
            bool isSkillApplyTypeAttack = (skillApplyType == SkillApplyType.DamageAP || skillApplyType == SkillApplyType.DamageHealth 
                                            || skillApplyType == SkillApplyType.Debuff);
            
            if(isStatusEffectForte == isSkillApplyTypeAttack) 
                castingApply.GetDamage().relativeDamageBonus *= 1.2f;
            else
                castingApply.GetDamage().relativeDamageBonus *= 0.9f;
        }
    }
}
