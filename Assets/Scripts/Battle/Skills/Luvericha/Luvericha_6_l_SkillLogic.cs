
namespace Battle.Skills {
    class Luvericha_6_l_SkillLogic : BaseSkillLogic {
        public override void ApplyAdditionalDamage(SkillInstanceData skillInstanceData) {
            Unit caster = skillInstanceData.GetCaster();
            Unit target = skillInstanceData.GetMainTarget();
            skillInstanceData.GetDamage().baseDamage = caster.GetMaxHealth();
            
            if(target.GetCurrentHealth() < caster.GetMaxHealth()) {
                skillInstanceData.GetDamage().baseDamage = target.GetCurrentHealth();
            }
            skillInstanceData.GetDamage().baseDamage *= 0.2f;
        }
    }
}
