
namespace Battle.Skills {
    class Grenev_5_l_SkillLogic : BasePassiveSkillLogic {
        public override void ApplyBonusDamageFromEachPassive(CastingApply castingApply) {
            Unit caster = castingApply.GetCaster();
            Unit target = castingApply.GetTarget();

            if(caster.GetHeight() <= target.GetHeight() - 3) {
                castingApply.GetDamage().relativeDamageBonus *= 1.5f;
            }
        }
    }
}
