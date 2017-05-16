using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enums;
using System.Linq;
using Battle.Damage;

namespace Battle.Skills {
    public class Curi_3_m_SkillLogic : BasePassiveSkillLogic {
        public override void ApplyBonusDamageFromEachPassive(SkillInstanceData skillInstanceData) {
            if (skillInstanceData.getSkill().GetName() == "수상한 덩어리")
                skillInstanceData.getDamage().relativeDamageBonus *= 0.6f;
        }
    }
}
