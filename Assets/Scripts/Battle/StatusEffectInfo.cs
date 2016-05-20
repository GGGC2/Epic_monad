using System.Collections;
using System;
using Enums;

public class StatusEffectInfo {

	public string owner;
	public Skill skill;
	public StatusEffect statusEffect;
	
	public string GetOwner()
	{
		return owner;
	}
	
	public Skill GetSkill()
	{
		return skill;
	}
	
	public StatusEffect GetStatusEffect()
	{
		return statusEffect;
	}
	
	public SkillInfo (string data)
	{
		string[] stringList = data.Split(',');

		this.owner = stringList[0];
		this.skill = stringList[1];
  
		string name = stringList[2];
		StatusEffectType statusEffectType = stringList[3];
        float degree = Single.Parse(stringList[4]);
        float amount = Single.Parse(stringList[5]);
        int remainPhase = Int32.Parse(stringList[6]);
		int cooldown = Int32.Parse(stringList[7]); 

		string effectName = stringList[8];
		EffectVisualType effectVisualType = (EffectVisualType)Enum.Parse(typeof(EffectVisualType), stringList[8]);
		EffectMoveType effectMoveType = (EffectMoveType)Enum.Parse(typeof(EffectMoveType), stringList[9]);
	
		this.statusEffect = new StatusEffect(name, statusEffectType, 
                                             degree, amount, remainPhase, coolDown, 
                                             effectName, effectVisualType, effectMoveType);
	}
}