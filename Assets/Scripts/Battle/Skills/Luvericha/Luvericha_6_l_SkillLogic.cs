
namespace Battle.Skills {
    class Luvericha_6_l_SkillLogic : BaseSkillLogic {
        public override void ApplyAdditionalDamage(CastingApply castingApply) {
            Unit caster = castingApply.GetCaster();
            Unit target = castingApply.GetTarget();
            castingApply.GetDamage().baseDamage = caster.GetMaxHealth();
            
            if(target.GetCurrentHealth() < caster.GetMaxHealth()) {
                castingApply.GetDamage().baseDamage = target.GetCurrentHealth();
            }
            castingApply.GetDamage().baseDamage *= 0.2f;
        }
    }
}
