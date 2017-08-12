
namespace Battle.Skills {
    class Luvericha_8_l_SkillLogic : BaseSkillLogic {
        public override void ApplyAdditionalDamage(SkillInstanceData skillInstanceData) {
            Unit target = skillInstanceData.GetTarget();
            skillInstanceData.GetDamage().baseDamage = target.GetCurrentActivityPoint() * 0.9f;
        }
    }
}
