﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enums;

namespace Battle.Skills {
    class Eugene_1_m_SkillLogic : BaseSkillLogic {
        private Dictionary<Unit, bool> TagUnitInRange(Unit target, StatusEffect statusEffect) {   //각 unit이 target이 가진 오오라 범위 안에 있을 경우 true 태그를 닮.
            TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();
            UnitManager unitManager = MonoBehaviour.FindObjectOfType<UnitManager>();
            List<Tile> auraRange = tileManager.GetTilesInRange(RangeForm.Diamond, target.position, 0, (int)statusEffect.GetAmount(), Direction.Down);
            Dictionary<Unit, bool> unitDictionary = new Dictionary<Unit, bool>();
            foreach (var unit in unitManager.GetAllUnits()) {
                if (auraRange.Contains(unit.GetTileUnderUnit())) unitDictionary.Add(unit, true);
                else    unitDictionary.Add(unit, false);
                
            }
            return unitDictionary;
        }
        public override bool TriggerStatusEffectApplied(StatusEffect statusEffect, Unit caster, Unit target) //StatusEffect가 적용될 때 발동. false를 반환할 경우 해당 StatusEffect가 적용되지 않음
        {
            if (statusEffect.GetIsInfinite() == true)  return false;  //오오라 효과는 적용하지 않음
            return true;
        }
        public override IEnumerator TriggerStatusEffectsAtActionEnd(Unit target, StatusEffect statusEffect) {
            Skill originSkill = statusEffect.GetOriginSkill();
            Dictionary<Unit, bool> unitInRangeDictionary = TagUnitInRange(target, statusEffect);
            foreach (var kv in unitInRangeDictionary) {
                Unit unit = kv.Key;
                StatusEffect alreadyAppliedEffect = unit.GetStatusEffectList().Find(se => (se.GetOriginSkillName() == "순백의 방패"
                                                        && se.GetStatusEffectType() != StatusEffectType.Aura));
                if (alreadyAppliedEffect != null && kv.Value == false) {            //원래 오오라 범위 안에 있었는데 액션 이후 벗어난 경우
                    alreadyAppliedEffect.DecreaseRemainStack();
                } else {
                    StatusEffect.FixedElement fixedElementOfAuraStatusEffect = skill.GetStatusEffectList().Find(se => se.actuals[0].statusEffectType != StatusEffectType.Aura);
                    StatusEffect auraStatusEffect = new StatusEffect(fixedElementOfAuraStatusEffect, statusEffect.GetCaster(), originSkill, null);
                    auraStatusEffect.flexibleElem.display.memorizedunit = target;
                    if (alreadyAppliedEffect == null) {                             //원래 오오라 효과를 받지 않았던 대상이 범위 안으로 들어왔을 경우
                        unit.GetStatusEffectList().Add(auraStatusEffect);
                    } else if (alreadyAppliedEffect.GetMemorizedUnit() != target) { //원래 오오라 효과를 받고 있었는데, 그 효과가 이 오오라를 가진 유닛으로 인한 것이 아닌 경우
                        alreadyAppliedEffect.AddRemainStack(1);
                    }
                }
            }
            yield return null;
        }
        public override bool TriggerStatusEffectRemoved(StatusEffect statusEffect, Unit target) {
            Dictionary<Unit, bool> unitInRangeDictionary = TagUnitInRange(target, statusEffect);
            foreach (var kv in unitInRangeDictionary) {
                Unit unit = kv.Key;
                if(kv.Value == true) {
                    StatusEffect statusEffectToRemove = unit.GetStatusEffectList().Find(se => (se.GetOriginSkillName() == "순백의 방패" 
                                                        && se.GetStatusEffectType() != StatusEffectType.Aura));
                    if(statusEffectToRemove != null) statusEffectToRemove.DecreaseRemainStack();
                }
            }
            return true;
        }
    }
}
