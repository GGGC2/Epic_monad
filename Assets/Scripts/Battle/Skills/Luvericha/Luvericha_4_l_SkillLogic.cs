
namespace Battle.Skills {
    class Luvericha_4_l_SkillLogic : BaseSkillLogic {
        public override void ApplyAdditionalDamage(CastingApply castingApply) {
            Unit target = castingApply.GetTarget();
            castingApply.GetDamage().baseDamage = target.GetCurrentActivityPoint() * 0.5f;
        }
    }
}
