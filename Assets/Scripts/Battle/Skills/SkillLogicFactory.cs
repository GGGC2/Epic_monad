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
