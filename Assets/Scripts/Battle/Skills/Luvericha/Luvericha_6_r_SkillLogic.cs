using System.Collections;

namespace Battle.Skills {
    class Luvericha_6_r_SkillLogic : BasePassiveSkillLogic {
        public override IEnumerator TriggerApplyingHeal(SkillInstanceData skillInstanceData) {
            Unit caster = skillInstanceData.GetCaster();
            yield return caster.RecoverHealth(skillInstanceData.GetDamage().resultDamage * 0.2f);
        }
    }
}
