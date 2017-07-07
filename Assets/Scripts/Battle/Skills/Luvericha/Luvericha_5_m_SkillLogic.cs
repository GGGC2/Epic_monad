using System.Collections;
using UnityEngine;

namespace Battle.Skills {
    class Luvericha_5_m_SkillLogic : BaseSkillLogic{
        public override IEnumerator TriggerShieldAttacked(Unit target, float amount) {
            BattleManager battleManager = MonoBehaviour.FindObjectOfType<BattleManager>();
            yield return battleManager.StartCoroutine(target.RecoverHealth(amount));
        }
    }
}
