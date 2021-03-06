﻿using System.Collections;
using UnityEngine;

namespace Battle.Skills {
    class Noel_0_l_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerWhenShieldWhoseCasterIsOwnerIsAttacked(Unit attacker, Unit shieldCaster, Unit target, float amount) {
			BattleManager battleManager = BattleData.battleManager;
			attacker.ApplyDamageByNonCasting(amount, shieldCaster, 0, 0, true, false, false);
        }
    }
}
