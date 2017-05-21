using Enums;

namespace Battle.Skills {
    public class Curi_3_1_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerDamaged(Unit target, int damage, Unit caster) {
            float percentHealthLost = (float)damage / (float)target.GetMaxHealth();
            float dexturity = target.GetStat(Stat.Dexturity);
            target.RecoverActionPoint((int)(dexturity * 0.01f));
        }
    }
}
