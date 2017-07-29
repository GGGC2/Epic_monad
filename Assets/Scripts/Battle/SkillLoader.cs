using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enums;
	
public class SkillLoader {
	public static List<ActiveSkill> MakeSkillList() {
		// 일단 임의로 하드코딩된 스킬셋을 사용.
		// 스킬에 필요한 정보는 아래와 같음. 
		// string name, int requireAP, int cooldown, 
		// float powerFactor,
		// SkillType skillType,
		// RangeForm firstRangeForm, int firstMinReach, int firstMaxReach, int firstWidth,
		// bool includeMyself,
		// RangeForm secondRangeForm, int secondMinReach, int secondMaxReach, int secondWidth,
		// SkillApplyType skillApplyType,
		// string effectName, EffectVisualType effectVisualType, EffectMoveType effectMoveType;
		
        List<ActiveSkill> skillList = new List<ActiveSkill>();
        /*
		Skill skill1 = new Skill("암흑 폭발", 40, 0, 
								 1.0f, 
								 SkillType.Point,
								 RangeForm.Square, 1, 4, 0, 
								 RangeForm.Square, 0, 1, 0,
								 SkillApplyType.Damage,
								 "darkBall", EffectVisualType.Area, EffectMoveType.Move);
		Skill skill2 = new Skill("태초의 빛", 35, 0, 
								 1.0f, 
								 SkillType.Point,
								 RangeForm.Cross, 0, 4, 0, 
								 RangeForm.Square, 0, 1, 0,
								 SkillApplyType.Heal,
								 "lightHeal", EffectVisualType.Individual, EffectMoveType.NonMove);
		Skill skill3 = new Skill("영혼의 불꽃", 100, 0, 
								 3.0f, 
								 SkillType.Area,
								 RangeForm.Straight, 1, 2, 0, 
								 RangeForm.Square, 0, 2, 0,
								 SkillApplyType.Damage,
								 "darkExplosion", EffectVisualType.Area, EffectMoveType.NonMove);
		Skill skill4 = new Skill("마력 보호막", 80, 0, 
								 1.5f, 
								 SkillType.Area,
								 RangeForm.Square, 0, 0, 0,
								 RangeForm.Square, 0, 0, 0,
								 SkillApplyType.Etc,
								 "lightShield", EffectVisualType.Individual, EffectMoveType.NonMove);
		Skill skill5 = new Skill("사념 포박", 160, 0, 
								 2.0f, 
								 SkillType.Point,
								 RangeForm.Square, 1, 3, 0, 
								 RangeForm.Square, 0, 1, 0,
								 SkillApplyType.Damage,
								 "darkBall", EffectVisualType.Area, EffectMoveType.Move);
		skillList.Add(skill1);
		skillList.Add(skill2);
		skillList.Add(skill3);
		skillList.Add(skill4);
		skillList.Add(skill5);
		*/
		return skillList;
	}
}
