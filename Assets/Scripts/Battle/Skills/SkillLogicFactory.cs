﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public static class SkillLogicFactory
{
	public static BaseSkillLogic Get(Skill skill)
	{
		switch (skill.GetName()) {
			// 레이나
			case "화염 폭발":
			return new Reina_1_l_SkillLogic();
			case "화염구":
			return new Reina_1_m_SkillLogic();
			case "지옥 불꽃":
			return new Reina_4_m_SkillLogic();
			// 리니안
			case "전자기 충격":
			return new Lenian_1_m_SkillLogic();
			// 영
			case "은빛 베기":
			return new Yeong_1_l_SkillLogic();
			
			case "조화진동":
			return new HarmonySkillLogic();
			case "생명력 흡수":
			return new LifeDrainSkillLogic();
			case "이매진 블릿":
			return new ImagineBulletSkillLogic();
			default:
			return new BaseSkillLogic();
		}
	}

	public static BasePassiveSkillLogic Get(List<PassiveSkill> passiveSkills)
	{
		List<BasePassiveSkillLogic> passiveSkillLogic = passiveSkills.Select(skill => Get(skill))
			.ToList();
		return new ListPassiveSkillLogic(passiveSkillLogic);
	}

	private static BasePassiveSkillLogic Get(PassiveSkill passiveSkill)
	{
		switch (passiveSkill.GetName())
		{
			// 레이나
			case "핀토스의 긍지":
			return new Reina_0_1_SkillLogic();
			case "불의 파동":
			return new Reina_2_m_SkillLogic();
			case "뭉치면 죽는다":
			return new Reina_5_l_SkillLogic();
			case "흩어져도 죽는다":
			return new Reina_5_m_SkillLogic();
			// 리니안
			case "연쇄 방전":
			return new Lenian_5_m_SkillLogic();
			case "입자 가속":
			return new Lenian_7_r_SkillLogic();
			// 영
			case "영회피":
			return new Yeong_0_1_SkillLogic();
			case "유법":
			return new Yeong_1_2_SkillLogic();
			default:
			return new BasePassiveSkillLogic();
		}
	}
}
}
