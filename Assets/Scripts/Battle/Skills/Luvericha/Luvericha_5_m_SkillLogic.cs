using System.Collections;
using UnityEngine;

namespace Battle.Skills {
    class Luvericha_5_m_SkillLogic : BaseSkillLogic{
        public override void TriggerShieldAttacked(Unit target, float amount) {
			BattleManager battleManager = BattleData.battleManager;
            target.RecoverHealth(amount);
        }
    }
}
