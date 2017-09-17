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
                if (previousStatusEffect != null && previousStatusEffect.display.toBeReplaced) { //������ previousStatusEffect�� ���ؼ��� ��ü ���θ� Ȯ����.
                                                                                                 //��, ��ü�Ǿ�� �ϴ� StatusEffect�� csv ���Ͽ��� �ٷ� ���� �ٿ� ������ ��.
                    unitStatusEffectList.Remove(previousStatusEffect);
                }
                if (statusEffectInfo.GetOriginSkillName().Equals(korName)) {
                    unitStatusEffectList.Add(statusEffectToAdd);
                }
            }
            previousStatusEffect = statusEffectToAdd;
        }
	}

	public string GetOwner(){return owner;}
	public int GetColumn() { return column; }
	public string GetName() {return korName;}
    public int GetRequireLevel() { return requireLevel;}
    public List<UnitStatusEffect.FixedElement> GetUnitStatusEffectList() {return unitStatusEffectList;}
}
