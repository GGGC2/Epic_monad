
namespace Battle.Skills {
    class Luvericha_1_r_SkillLogic : BaseSkillLogic{
        public override void ApplyAdditionalDamage(CastingApply castingApply) {
            if(castingApply.GetCaster() == castingApply.GetTarget()) {
                castingApply.GetDamage().relativeDamageBonus = 0;
            }
        }
    }
}
