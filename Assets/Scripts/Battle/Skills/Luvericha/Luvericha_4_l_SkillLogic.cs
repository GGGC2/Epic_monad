
namespace Battle.Skills {
    class Luvericha_4_l_SkillLogic : BaseSkillLogic {
        public override void ApplyAdditionalDamage(SkillInstanceData skillInstanceData) {
            Unit target = skillInstanceData.GetMainTarget();
            skillInstanceData.GetDamage().baseDamage = target.activityPoint * 0.5f;
        }
    }
}
