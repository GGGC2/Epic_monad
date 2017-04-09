using UnityEngine;
using System.Linq;

namespace Battle.Skills {
    public class Curi_3_1_SkillLogic : BasePassiveSkillLogic {

        public override void triggerDamaged(Unit unit, int damage) {
            float percentHealthLost = (float)damage / (float)unit.GetMaxHealth();
            float dexturity = unit.GetStat(Enums.Stat.Dexturity);
            unit.RecoverAP((int)(dexturity * 0.01f));
        }
    }
}
