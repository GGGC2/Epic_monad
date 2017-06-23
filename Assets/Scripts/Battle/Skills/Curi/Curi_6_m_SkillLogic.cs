using System.Collections;
using UnityEngine;

namespace Battle.Skills {
    public class Curi_6_m_SkillLogic : BaseSkillLogic {
        public override IEnumerator ActionInDamageRoutine(SkillInstanceData skillInstanceData) {
            BattleManager battleManager = MonoBehaviour.FindObjectOfType<BattleManager>();
            Unit target = skillInstanceData.GetMainTarget();
            skillInstanceData.GetDamage().resultDamage = target.GetMaxHealth() * 0.1f;
            yield return battleManager.StartCoroutine(target.Damaged(skillInstanceData, true));
        }
    }
}
