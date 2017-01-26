using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Skills
{
public static class SkillLogicFactory
{
	public static BaseSkillLogic Get(Skill skill)
	{
		switch (skill.GetName()) {
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
}
}