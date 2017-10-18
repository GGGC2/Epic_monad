using Enums;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using Battle.Skills;

public class PassiveSkill : Skill{
	List<UnitStatusEffect.FixedElement> unitStatusEffectList = new List<UnitStatusEffect.FixedElement>();
	public BasePassiveSkillLogic SkillLogic {
		get { return SkillLogicFactory.Get (this); }
	}

	public PassiveSkill(string skillData){
		StringParser commaParser = new StringParser(skillData, '\t');
		
		GetCommonSkillData(commaParser);
		skillDataText = commaParser.Consume();
		firstTextValueType = commaParser.ConsumeEnum<Stat>();
		firstTextValueCoef = commaParser.ConsumeFloat();
        firstTextValueBase = commaParser.ConsumeFloat();
		secondTextValueType = commaParser.ConsumeEnum<Stat>();
		secondTextValueCoef = commaParser.ConsumeFloat();
        secondTextValueBase = commaParser.ConsumeFloat();
	}

	public void ApplyUnitStatusEffectList(List<UnitStatusEffectInfo> statusEffectInfoList, int partyLevel){
        UnitStatusEffect.FixedElement previousStatusEffect = null;
        foreach (var statusEffectInfo in statusEffectInfoList) {
            UnitStatusEffect.FixedElement statusEffectToAdd = statusEffectInfo.GetStatusEffect();
            if (statusEffectInfo.GetRequireLevel() <= partyLevel) {
                if (previousStatusEffect != null && previousStatusEffect.display.toBeReplaced) { //이전의 previousStatusEffect에 대해서만 대체 여부를 확인함.
                                                                                                 //즉, 대체되어야 하는 StatusEffect는 csv 파일에서 바로 다음 줄에 만들어야 함.
                    unitStatusEffectList.Remove(previousStatusEffect);
                }
                if (statusEffectInfo.GetOriginSkillName().Equals(korName) &&
                    !unitStatusEffectList.Contains(statusEffectToAdd)) {    // 같은 패시브를 가진 유닛이 여러 개일 때 중복으로 들어가는 것 방지
                    unitStatusEffectList.Add(statusEffectToAdd);
                }
            }
            previousStatusEffect = statusEffectToAdd;
        }
	}

	public string GetOwner(){return owner;}
	public int GetColumn() { return row; }
	public string GetName() {return korName;}
    public int GetRequireLevel() { return requireLevel;}
    public List<UnitStatusEffect.FixedElement> GetUnitStatusEffectList() {return unitStatusEffectList;}
}
