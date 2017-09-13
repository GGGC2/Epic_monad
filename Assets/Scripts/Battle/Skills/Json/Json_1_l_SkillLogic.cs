
namespace Battle.Skills {
    class Json_1_l_SkillLogic : BaseSkillLogic {
        public override void ApplyAdditionalDamage(CastingApply castingApply) {
            Unit target = castingApply.GetTarget();
            DamageCalculator.AttackDamage damage = castingApply.GetDamage();
            
            UnitStatusEffect mark = target.StatusEffectList.Find(se => se.GetDisplayName() == "표식");
            int stack = 0;
            if(mark != null)    stack = mark.GetRemainStack();
            damage.relativeDamageBonus *= 1 + stack * 0.1f;
        }
    }
}
