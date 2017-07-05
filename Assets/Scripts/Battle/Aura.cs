using System;
using System.Collections.Generic;
using UnityEngine;
using Enums;
using System.Linq;
using System.Text;

class Aura{
    private static Dictionary<Unit, bool> TagUnitInRange(Unit target, StatusEffect statusEffect) {   //각 unit이 target이 가진 오오라 범위 안에 있을 경우 true 태그를 닮.
        TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();
        UnitManager unitManager = MonoBehaviour.FindObjectOfType<UnitManager>();
        Dictionary<Unit, bool> unitDictionary = new Dictionary<Unit, bool>();
        List<Tile> auraRange = tileManager.GetTilesInRange(RangeForm.Diamond, target.GetPosition(), 0, 
                                    (int)statusEffect.GetAmountOfType(StatusEffectType.Aura), 0, Direction.Down);
        foreach (var unit in unitManager.GetAllUnits()) {
            if (auraRange.Contains(unit.GetTileUnderUnit())) unitDictionary.Add(unit, true);
            else unitDictionary.Add(unit, false);
        }
        return unitDictionary;
    }
    public static bool Update(Unit owner, StatusEffect statusEffect) {
        Skill originSkill = statusEffect.GetOriginSkill();
        PassiveSkill originPassiveSkill = statusEffect.GetOriginPassiveSkill();
        Dictionary<Unit, bool> unitInRangeDictionary = TagUnitInRange(owner, statusEffect);
        foreach (var kv in unitInRangeDictionary) {
            Unit unit = kv.Key;
            if(unit == owner)   continue;
            StatusEffect alreadyAppliedEffect = unit.GetStatusEffectList().Find(se => (se.GetOriginSkill() == originSkill
                                                    && se.GetOriginPassiveSkill() == originPassiveSkill
                                                    && se.IsOfType(StatusEffectType.Aura)));
            if (alreadyAppliedEffect != null && kv.Value == false) {                    //원래 오오라 범위 안에 있었는데 액션 이후 벗어난 경우
                alreadyAppliedEffect.DecreaseRemainStack();
                alreadyAppliedEffect.GetMemorizedUnits().Remove(owner);
            } else if(kv.Value == true){
                StatusEffect.FixedElement fixedElementOfAuraStatusEffect = null;
                if (originSkill != null)
                    fixedElementOfAuraStatusEffect = originSkill.GetStatusEffectList().Find(se => 
                                                        se.actuals[0].statusEffectType != StatusEffectType.Aura);
                if (originPassiveSkill != null)
                    fixedElementOfAuraStatusEffect = originPassiveSkill.GetStatusEffectList().Find(se =>
                                                        se.actuals[0].statusEffectType != StatusEffectType.Aura);

                if (alreadyAppliedEffect == null) {                                     //원래 오오라 효과를 받지 않았던 대상이 범위 안으로 들어왔을 경우
                    StatusEffect auraStatusEffect = new StatusEffect(fixedElementOfAuraStatusEffect, statusEffect.GetCaster(), unit, originSkill, originPassiveSkill);
                    auraStatusEffect.GetMemorizedUnits().Add(owner); 
                    unit.GetStatusEffectList().Add(auraStatusEffect);
                } else if (!alreadyAppliedEffect.GetMemorizedUnits().Contains(owner)) {  //원래 오오라 효과를 받고 있었는데, 그 효과가 이 오오라를 가진 유닛으로 인한 것이 아닌 경우
                    alreadyAppliedEffect.AddRemainStack(1);
                    alreadyAppliedEffect.GetMemorizedUnits().Add(owner);
                }
            }
        }
        
        return true;
    }
    public static void TriggerOnApplied(StatusEffect statusEffect, Unit caster, Unit target) //StatusEffect가 적용될 때 발동. false를 반환할 경우 해당 StatusEffect가 적용되지 않음
    {
        if (statusEffect.IsOfType(StatusEffectType.Aura)) {
            statusEffect.GetMemorizedUnits().Add(target);
        }
    }
    public static void TriggerOnRemoved(Unit owner, StatusEffect statusEffect) {
        if (statusEffect.IsOfType(StatusEffectType.Aura)) {
            Dictionary<Unit, bool> unitInRangeDictionary = TagUnitInRange(owner, statusEffect);
            foreach (var kv in unitInRangeDictionary) {
                Unit unit = kv.Key;
                if (kv.Value == true) {
                    StatusEffect statusEffectToRemove = unit.GetStatusEffectList().Find(se => (se.GetOriginSkill() == statusEffect.GetOriginSkill()
                                                        && se.GetOriginPassiveSkill() == statusEffect.GetOriginPassiveSkill()
                                                        && se.IsOfType(StatusEffectType.Aura)));
                    if (statusEffectToRemove != null) {
                        statusEffectToRemove.DecreaseRemainStack();
                        statusEffectToRemove.GetMemorizedUnits().Remove(owner);
                    }
                }
            }
        }
    }

}
