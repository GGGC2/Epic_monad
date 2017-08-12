
using Enums;

namespace Battle.Skills {
    class Grenev_3_r_SkillLogic : BasePassiveSkillLogic{
        public override void ApplyBonusDamageFromEachPassive(CastingApply castingApply) {
            Unit caster = castingApply.GetCaster();
            Unit target = castingApply.GetTarget();

            if (caster.GetStat(Stat.Power) > target.GetStat(Stat.Power)){
                castingApply.GetDamage().relativeDamageBonus *= 1.25f;
            }
        }
    }
}
