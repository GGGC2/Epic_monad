
namespace Battle.Skills {
    class Grenev_5_l_SkillLogic : BasePassiveSkillLogic {
        public override void ApplyBonusDamageFromEachPassive(SkillInstanceData skillInstanceData) {
            Unit caster = skillInstanceData.GetCaster();
            Unit target = skillInstanceData.GetMainTarget();

            if(caster.GetHeight() <= target.GetHeight() - 3) {
                skillInstanceData.GetDamage().relativeDamageBonus *= 1.5f;
            }
        }
    }
}
