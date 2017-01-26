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
			default:
			return new BaseSkillLogic();
		}
	}
}
}