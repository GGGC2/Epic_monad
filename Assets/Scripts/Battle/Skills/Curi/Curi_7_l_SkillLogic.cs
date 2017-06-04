using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Battle.Damage;

namespace Battle.Skills {
    public class Curi_7_l_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerStart(Unit caster) {
            StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
        }
        public override float GetStatusEffectVar(StatusEffect statusEffect, int i, Unit caster) {
            UnitManager unitManager = MonoBehaviour.FindObjectOfType<UnitManager>();
            List<Unit> unitsExceptThis = new List<Unit>();
            foreach(Unit unit in unitManager.GetAllUnits()) {
                if(unit != caster) 
                    unitsExceptThis.Add(unit);
            }
            int distance = unitsExceptThis.Min(x => Utility.GetDistance(caster.GetPosition(), x.GetPosition()));
            return distance;
        }
    }
}