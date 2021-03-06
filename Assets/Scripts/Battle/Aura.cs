﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Enums;
using System.Linq;
using System.Text;
using Battle.Damage;

class Aura{
    private static Dictionary<Unit, bool> TagUnitInRange(Unit target, UnitStatusEffect statusEffect) {   //각 unit이 target이 가진 오오라 범위 안에 있을 경우 true 태그를 닮.
		TileManager tileManager = BattleData.tileManager;
		UnitManager unitManager = BattleData.unitManager;
        Dictionary<Unit, bool> unitDictionary = new Dictionary<Unit, bool>();
        List<Tile> auraRange = tileManager.GetTilesInRange(RangeForm.Diamond, target.GetPosition(), 0, 
                                    (int)statusEffect.GetAmountOfType(StatusEffectType.Aura), 0, Direction.Down);
        foreach (var unit in unitManager.GetAllUnits()) {
            if (auraRange.Contains(unit.GetTileUnderUnit())) unitDictionary.Add(unit, true);
            else unitDictionary.Add(unit, false);
        }
        return unitDictionary;
    }
    public static bool Update(Unit owner, UnitStatusEffect statusEffect) {
        Skill originSkill = statusEffect.GetOriginSkill();
        Dictionary<Unit, bool> unitInRangeDictionary = TagUnitInRange(owner, statusEffect);
        foreach (var kv in unitInRangeDictionary) {
            Unit unit = kv.Key;
            if(unit == owner)   continue;
            UnitStatusEffect alreadyAppliedEffect = unit.StatusEffectList.Find(se => (se.GetOriginSkill() == originSkill
                                                    && !se.IsOfType(StatusEffectType.Aura)));
            if (alreadyAppliedEffect != null && kv.Value == false) {                    //원래 오오라 범위 안에 있었는데 액션 이후 벗어난 경우
                alreadyAppliedEffect.GetMemorizedUnits().Remove(owner);
                alreadyAppliedEffect.SetRemainStack(alreadyAppliedEffect.GetMemorizedUnits().Count);
            } else if(kv.Value){
                UnitStatusEffect.FixedElement fixedElementOfAuraStatusEffect = null;
                if (originSkill is ActiveSkill)
                    fixedElementOfAuraStatusEffect = ((ActiveSkill)originSkill).GetUnitStatusEffectList().Find(se => 
                                                        se.actuals[0].statusEffectType != StatusEffectType.Aura);
                if (originSkill is PassiveSkill)
                    fixedElementOfAuraStatusEffect = ((PassiveSkill)originSkill).GetUnitStatusEffectList().Find(se =>
                                                        se.actuals[0].statusEffectType != StatusEffectType.Aura);

                if (alreadyAppliedEffect == null) {                                     //원래 오오라 효과를 받지 않았던 대상이 범위 안으로 들어왔을 경우
                    UnitStatusEffect auraStatusEffect = new UnitStatusEffect(fixedElementOfAuraStatusEffect, statusEffect.GetCaster(), unit, originSkill);
                    auraStatusEffect.GetMemorizedUnits().Add(owner); 
                    List<UnitStatusEffect> auraStatusEffectList = new List<UnitStatusEffect>();
                    auraStatusEffectList.Add(auraStatusEffect);
                    StatusEffector.AttachStatusEffect(owner, auraStatusEffectList, unit);
                    // LogManager.Instance.Record(new StatusEffectLog(auraStatusEffect, StatusEffectChangeType.Attach, 0, 0, 0));
                    //unit.StatusEffectList.Add(auraStatusEffect);
                } else if (!alreadyAppliedEffect.GetMemorizedUnits().Contains(owner)) {  //원래 오오라 효과를 받고 있었는데, 그 효과가 이 오오라를 가진 유닛으로 인한 것이 아닌 경우
                    alreadyAppliedEffect.AddRemainStack(1);
                    alreadyAppliedEffect.GetMemorizedUnits().Add(owner);
                }
            }
        }
        
        return true;
    }
    public static void TriggerOnApplied(UnitStatusEffect statusEffect, Unit caster, Unit target) //StatusEffect가 적용될 때 발동. false를 반환할 경우 해당 StatusEffect가 적용되지 않음
    {
        statusEffect.GetMemorizedUnits().Add(target);
    }
    public static void TriggerOnRemoved(Unit owner, UnitStatusEffect statusEffect) {
        if (statusEffect.IsOfType(StatusEffectType.Aura)) {
            Dictionary<Unit, bool> unitInRangeDictionary = TagUnitInRange(owner, statusEffect); //각 유닛에 대해 범위 안에 있는지 확인
            foreach (var kv in unitInRangeDictionary) {
                Unit unit = kv.Key;
                if (kv.Value) {     //유닛이 범위 안에 있다면
                    UnitStatusEffect statusEffectToRemove = unit.StatusEffectList.Find(se => 
                                (se.GetOriginSkill() == statusEffect.GetOriginSkill()   && !se.IsOfType(StatusEffectType.Aura)));   //Aura 그 자체가 아닌 효과를 찾아서
                    if (statusEffectToRemove != null) {
                        statusEffectToRemove.GetMemorizedUnits().Remove(owner);
                        statusEffectToRemove.SetRemainStack(statusEffectToRemove.GetMemorizedUnits().Count);    // 스택을 감소시킨다.
                    }
                }
            }
        }
    }

}
