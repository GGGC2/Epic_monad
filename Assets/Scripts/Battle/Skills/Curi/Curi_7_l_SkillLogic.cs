using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Battle.Damage;

namespace Battle.Skills {
    public class Curi_7_l_SkillLogic : BasePassiveSkillLogic {
        /*public override void TriggerStart(Unit caster) {
            StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
        }*/
        public override void TriggerActionEnd(Unit caster) {
            UnitManager unitManager = MonoBehaviour.FindObjectOfType<UnitManager>();
            List<Unit> unitsExceptCaster = new List<Unit>();
            foreach(Unit unit in unitManager.GetAllUnits()) {
                if(unit != caster)  unitsExceptCaster.Add(unit);
            }
            
            int distance = unitsExceptCaster.Min(x => Utility.GetDistance(caster.GetPosition(), x.GetPosition()));
            if(distance >= 25 ) distance =25;

            StatusEffect statusEffect = caster.GetStatusEffectList().Find(se => se.GetOriginSkillName() == "제한 구역");
            if(statusEffect == null) {
                foreach(var SE in passiveSkill.GetStatusEffectList()) {
                    Debug.Log(SE.display.originSkillName);
                }
                StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
            }
            statusEffect = caster.GetStatusEffectList().Find(se => se.GetOriginSkillName() == "제한 구역");
            StatusEffect.FixedElement.ActualElement actual = statusEffect.fixedElem.actuals[0];
            
            statusEffect.SetAmount(actual.seCoef * distance + actual.seBase);
        }
    }
}