using Enums;
using Battle.Damage;

namespace Battle.Skills {
    public class Curi_0_1_SkillLogic : BasePassiveSkillLogic {
        public override void ApplyBonusDamageFromEachPassive(SkillInstanceData skillInstanceData) {
            Unit caster = skillInstanceData.GetCaster();
            bool allTargetsHaveSameElement = true;
            bool firstTarget = true;
            Element element = Element.None;

            StatusEffect originalStatusEffect = caster.GetStatusEffectList().Find(se => se.GetOriginSkillName()=="정제");
            
            foreach (var target in skillInstanceData.GetTargets()) {
                if (firstTarget == true) {
                    firstTarget = false;
                    element = target.GetElement();
                    continue;
                }
                if (target.GetElement() != element) {
                    allTargetsHaveSameElement = false;
                    break;
                }
            }
            
            if(allTargetsHaveSameElement && element == originalStatusEffect.GetElement()) {
                if(originalStatusEffect != null)
                    skillInstanceData.GetDamage().relativeDamageBonus *= 1 + (0.05f * originalStatusEffect.GetRemainStack());
                StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
            }
            else if(allTargetsHaveSameElement) {
                caster.RemoveStatusEffect(originalStatusEffect);
                StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
                caster.GetStatusEffectList().Find(se => se.GetOriginSkillName()=="정제").flexibleElem.display.element = element;
            }
            else 
                caster.RemoveStatusEffect(originalStatusEffect);
        }
    }
}