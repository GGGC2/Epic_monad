using UnityEngine;
using System.Linq;

namespace Battle.Skills {
    public class Curi_3_1_SkillLogic : BasePassiveSkillLogic {

        public override void TriggerDamaged(Unit unit, int damage) {
            float percentHealthLost = (float)damage / (float)unit.GetMaxHealth();
            float dexturity = unit.GetStat(Enums.Stat.Dexturity);
            unit.RecoverActionPoint((int)(dexturity * 0.01f));
        }
    }
}
