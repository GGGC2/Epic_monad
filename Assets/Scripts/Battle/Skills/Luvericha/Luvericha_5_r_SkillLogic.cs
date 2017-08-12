
namespace Battle.Skills {
    class Luvericha_5_r_SkillLogic : BasePassiveSkillLogic {
        public override void ApplyBonusDamageFromEachPassive(CastingApply castingApply) {
            castingApply.GetDamage().relativeDamageBonus *= 1 + (0.05f * castingApply.GetTargetCount());
        }
    }
}
