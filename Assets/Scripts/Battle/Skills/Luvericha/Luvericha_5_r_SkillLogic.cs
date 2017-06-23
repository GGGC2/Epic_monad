
namespace Battle.Skills {
    class Luvericha_5_r_SkillLogic : BasePassiveSkillLogic {
        public override void ApplyBonusDamageFromEachPassive(SkillInstanceData skillInstanceData) {
            skillInstanceData.GetDamage().relativeDamageBonus *= 1 + (0.05f * skillInstanceData.GetTargetCount());
        }
    }
}
