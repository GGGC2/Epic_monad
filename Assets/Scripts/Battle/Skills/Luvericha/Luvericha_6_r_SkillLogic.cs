using System.Collections;

namespace Battle.Skills {
    class Luvericha_6_r_SkillLogic : BasePassiveSkillLogic {
        public override IEnumerator TriggerApplyingHeal(CastingApply castingApply) {
            Unit caster = castingApply.GetCaster();
            yield return caster.RecoverHealth(castingApply.GetDamage().resultDamage * 0.2f);
        }
    }
}
