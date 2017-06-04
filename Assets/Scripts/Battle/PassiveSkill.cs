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
        foreach (StatusEffectInfo statusEffectInfo in statusEffectInfoList) {
            if (statusEffectInfo.GetOriginSkillName().Equals(name) && statusEffectInfo.GetRequireLevel() <= partyLevel) {
                StatusEffect.FixedElement statusEffectToAdd = statusEffectInfo.GetStatusEffect();
                foreach (StatusEffect.FixedElement statusEffect in statusEffectList) {
                    if (statusEffect.display.originSkillName == statusEffectToAdd.display.originSkillName) {
                        statusEffectList.Remove(statusEffect);  //강화된 statusEffect로 대체
                    }
                }
                statusEffectList.Add(statusEffectToAdd);
            }
        }
	}

	public string GetOwner(){return owner;}
	public int GetColumn() { return column; }
	public string GetName() {return name;}
    public int GetRequireLevel() { return requireLevel;}
    public List<StatusEffect.FixedElement> GetStatusEffectList() {return statusEffectList;}
}
