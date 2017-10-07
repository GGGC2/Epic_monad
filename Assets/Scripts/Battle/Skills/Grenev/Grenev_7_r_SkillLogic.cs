using System.Collections;
using UnityEngine;
using Enums;

namespace Battle.Skills {
    class Grenev_7_r_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnKill(HitInfo hitInfo, Unit deadUnit) {
            Unit caster = hitInfo.caster;
			BattleManager battleManager = BattleData.battleManager;
            caster.RecoverActionPoint((int)(caster.GetStat(Stat.Agility) * 0.6f));
        }
    }
}
