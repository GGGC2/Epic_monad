using Enums;
using UnityEngine;

namespace Battle.Skills {
    public class Curi_3_1_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerDamaged(Unit target, int damage, Unit caster) {
            float percentHealthLost = (float)damage / (float)target.GetMaxHealth();
            float dexturity = target.GetStat(Stat.Agility);
            MonoBehaviour.FindObjectOfType<BattleManager>().StartCoroutine(target.RecoverActionPoint((int)(dexturity * percentHealthLost)));
        }
    }
}
