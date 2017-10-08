using System.Collections;

namespace Battle.Skills {
    class Luvericha_6_r_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerApplyingHeal(CastingApply castingApply) {
            Unit caster = castingApply.GetCaster();
            caster.RecoverHealth(castingApply.GetDamage().resultDamage * 0.2f);
        }
    }
}
