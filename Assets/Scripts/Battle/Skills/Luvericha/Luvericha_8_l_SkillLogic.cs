
namespace Battle.Skills {
    class Luvericha_8_l_SkillLogic : BaseSkillLogic {
        public override void ApplyAdditionalDamage(CastingApply castingApply) {
            Unit target = castingApply.GetTarget();
            castingApply.GetDamage().baseDamage = target.GetCurrentActivityPoint() * 0.9f;
        }
    }
}
