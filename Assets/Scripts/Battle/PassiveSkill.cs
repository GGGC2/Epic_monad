using Enums;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public class PassiveSkill {

	// base info.
	string owner;
	int column;
	string name;
    int requireLevel;
	List<StatusEffect.FixedElement> statusEffectList = new List<StatusEffect.FixedElement>();
	
	public PassiveSkill(string owner, int column, string name, int requireLevel)
	{
		this.owner = owner;
		this.column = column;
		this.name = name;
        this.requireLevel = requireLevel;
	}

	public void ApplyStatusEffectList(List<StatusEffectInfo> statusEffectInfoList, int partyLevel)
	{
        StatusEffect.FixedElement previousStatusEffect = null;
        foreach (StatusEffectInfo statusEffectInfo in statusEffectInfoList) {
            StatusEffect.FixedElement statusEffectToAdd = statusEffectInfo.GetStatusEffect();
            if (statusEffectInfo.GetOriginSkillName().Equals(name) && statusEffectInfo.GetRequireLevel() <= partyLevel) {

                if (previousStatusEffect != null && previousStatusEffect.display.originSkillName == statusEffectToAdd.display.originSkillName
                    && previousStatusEffect.display.toBeReplaced) { //������ previousStatusEffect�� ���ؼ��� ��ü ���θ� Ȯ����.
                                                                    //��, ��ü�Ǿ�� �ϴ� StatusEffect�� csv ���Ͽ��� �ٷ� ���� �ٿ� ������ ��.
                    statusEffectList.Remove(previousStatusEffect);
                }

                statusEffectList.Add(statusEffectToAdd);
            }
            previousStatusEffect = statusEffectToAdd;
        }
	}

	public string GetOwner(){return owner;}
	public int GetColumn() { return column; }
	public string GetName() {return name;}
    public int GetRequireLevel() { return requireLevel;}
    public List<StatusEffect.FixedElement> GetStatusEffectList() {return statusEffectList;}
}
