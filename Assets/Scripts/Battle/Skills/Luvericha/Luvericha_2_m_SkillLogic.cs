using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Battle.Damage;
using UnityEngine;
using Enums;

namespace Battle.Skills {
    class Luvericha_2_m_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnStart(Unit caster) {
            StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
        }
        public override float GetStatusEffectVar(UnitStatusEffect statusEffect, int i, Unit caster, Unit owner) {
            UnitManager unitManager = MonoBehaviour.FindObjectOfType<UnitManager>();
            int numberOfInjuredAlly = 0;
            foreach(var unit in unitManager.GetAllUnits()) {
                if (unit.GetSide() == Side.Ally) {
                    float hpPercent = (float)unit.GetCurrentHealth() / (float) unit.GetMaxHealth();
                    if(hpPercent <= 0.4f) {
                        numberOfInjuredAlly ++;
                    }
                }
            }
            return numberOfInjuredAlly;
        }
    }
}
