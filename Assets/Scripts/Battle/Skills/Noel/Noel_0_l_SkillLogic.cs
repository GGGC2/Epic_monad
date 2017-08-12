using System.Collections;
using UnityEngine;

namespace Battle.Skills {
    class Noel_0_l_SkillLogic : BasePassiveSkillLogic {
        public override IEnumerator TriggerWhenShieldWhoseCasterIsOwnerIsAttacked(Unit attacker, Unit shieldCaster, Unit target, float amount) {
            BattleManager battleManager = MonoBehaviour.FindObjectOfType<BattleManager>();
			yield return battleManager.StartCoroutine(attacker.ApplyDamageByNonCasting(amount, shieldCaster, 0, 0, true, false, false));
        }
    }
}
