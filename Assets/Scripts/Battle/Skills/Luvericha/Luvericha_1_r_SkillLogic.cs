
namespace Battle.Skills {
    class Luvericha_1_r_SkillLogic : BaseSkillLogic{
        public override void ApplyAdditionalDamage(SkillInstanceData skillInstanceData) {
            if(skillInstanceData.GetCaster() == skillInstanceData.GetTarget()) {
                skillInstanceData.GetDamage().relativeDamageBonus = 0;
            }
        }
    }
}
