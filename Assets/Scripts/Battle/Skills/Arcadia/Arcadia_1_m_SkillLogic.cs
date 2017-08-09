using System.Collections.Generic;
using Enums;
using UnityEngine;
namespace Battle.Skills {
    class Arcadia_1_m_SkillLogic : BaseSkillLogic {
        public override float GetStatusEffectVar(UnitStatusEffect statusEffect, int i, Unit caster, Unit owner) {
            if(i==0)
                return caster.GetStat(Stat.MaxHealth);
            else
                return (float)(GameData.PartyData.level * 1.2 + 87);
        }
    }
}
