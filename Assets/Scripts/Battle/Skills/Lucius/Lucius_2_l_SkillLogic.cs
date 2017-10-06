
namespace Battle.Skills {
    class Lucius_2_l_SkillLogic : BasePassiveSkillLogic {
        public override void ApplyBonusDamageFromEachPassive(CastingApply castingApply) {
            DamageCalculator.AttackDamage damage = castingApply.GetDamage();

            if(!damage.HasTacticalBonus)
                damage.relativeDamageBonus *= 1.2f;
        }
    }
}
