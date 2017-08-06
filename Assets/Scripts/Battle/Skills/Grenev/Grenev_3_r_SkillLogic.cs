
using Enums;

namespace Battle.Skills {
    class Grenev_3_r_SkillLogic : BasePassiveSkillLogic{
        public override void ApplyBonusDamageFromEachPassive(SkillInstanceData skillInstanceData) {
            Unit caster = skillInstanceData.GetCaster();
            Unit target = skillInstanceData.GetMainTarget();

            if (caster.GetStat(Stat.Power) > target.GetStat(Stat.Power)){
                skillInstanceData.GetDamage().relativeDamageBonus *= 1.25f;
            }
        }
    }
}
