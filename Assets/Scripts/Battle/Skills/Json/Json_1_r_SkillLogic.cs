
namespace Battle.Skills {
    class Json_1_r_SkillLogic : BaseSkillLogic {
        public override void ApplyAdditionalDamage(CastingApply castingApply) {
            Unit caster = castingApply.GetCaster();
            Unit target = castingApply.GetTarget();
            DamageCalculator.AttackDamage damage = castingApply.GetDamage();
            UnitStatusEffect mark = target.StatusEffectList.Find(se => se.GetDisplayName() == "표식");
            if (mark == null) {
                damage.relativeDamageBonus *= 0;
            }
        }
    }
}
