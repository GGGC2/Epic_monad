using Enums;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public class PassiveSkill : Skill{
	List<StatusEffect.FixedElement> statusEffectList = new List<StatusEffect.FixedElement>();
	
	/*public PassiveSkill(string owner, int column, string name, int requireLevel){
		this.owner = owner;
		this.column = column;
		this.korName = name;
        this.requireLevel = requireLevel;
	}*/

	public PassiveSkill(string skillData){
		CommaStringParser commaParser = new CommaStringParser(skillData);
		
		GetCommonSkillData(commaParser);
		skillDataText = commaParser.Consume();
		firstTextValueType = commaParser.ConsumeEnum<Stat>();
		firstTextValueCoef = commaParser.ConsumeFloat();
		secondTextValueType = commaParser.ConsumeEnum<Stat>();
		secondTextValueCoef = commaParser.ConsumeFloat();
	}

	public void ApplyStatusEffectList(List<StatusEffectInfo> statusEffectInfoList, int partyLevel)
	{
        StatusEffect.FixedElement previousStatusEffect = null;
        foreach (StatusEffectInfo statusEffectInfo in statusEffectInfoList) {
            StatusEffect.FixedElement statusEffectToAdd = statusEffectInfo.GetStatusEffect();
            if (statusEffectInfo.GetRequireLevel() <= partyLevel) {
                if (previousStatusEffect != null && previousStatusEffect.display.toBeReplaced) { //������ previousStatusEffect�� ���ؼ��� ��ü ���θ� Ȯ����.
                                                                                                 //��, ��ü�Ǿ�� �ϴ� StatusEffect�� csv ���Ͽ��� �ٷ� ���� �ٿ� ������ ��.
                    statusEffectList.Remove(previousStatusEffect);
                }
                if (statusEffectInfo.GetOriginSkillName().Equals(korName)) {
                    statusEffectList.Add(statusEffectToAdd);
                }
            }
            previousStatusEffect = statusEffectToAdd;
        }
	}

	public string GetOwner(){return owner;}
	public int GetColumn() { return column; }
	public string GetName() {return korName;}
    public int GetRequireLevel() { return requireLevel;}
    public List<StatusEffect.FixedElement> GetStatusEffectList() {return statusEffectList;}
}
