using System.Collections;
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
			return new Lenien_1_m_SkillLogic();
			// 영
			case "은빛 베기":
			return new Yeong_1_l_SkillLogic();
			// 카샤스티
			case "더블 샷":
			return new Kashyasty_1_l_SkillLogic();

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
		BasePassiveSkillLogic passiveSkillLogic = null;
		switch (passiveSkill.GetName())
		{
			// 레이나
			case "핀토스의 긍지":
			passiveSkillLogic = new Reina_0_1_SkillLogic();
			break;
			case "불의 파동":
			passiveSkillLogic = new Reina_2_m_SkillLogic();
			break;
			case "뭉치면 죽는다":
			passiveSkillLogic = new Reina_5_l_SkillLogic();
			break;
			case "흩어져도 죽는다":
			passiveSkillLogic = new Reina_5_m_SkillLogic();
			break;
			// 리니안
			case "연쇄 방전":
			passiveSkillLogic = new Lenien_5_m_SkillLogic();
			break;
			case "입자 가속":
			passiveSkillLogic = new Lenien_7_r_SkillLogic();
			break;
			// 영
			case "영회피":
			passiveSkillLogic = new Yeong_0_1_SkillLogic();
			break;
			case "유법":
			passiveSkillLogic = new Yeong_1_2_SkillLogic();
			break;
			case "동체 시력":
			passiveSkillLogic = new Yeong_1_r_SkillLogic();
			break;
			// 에렌
			case "초월":
			passiveSkillLogic = new Eren_3_m_SkillLogic();
			break;
			case "배척받는 자":
			passiveSkillLogic = new Eren_5_l_SkillLogic();
			break;
			case "진형 붕괴":
			passiveSkillLogic = new Eren_6_m_SkillLogic();
			break;
			// 카샤스티
			case "장미 속의 가시":
			passiveSkillLogic = new Kashyasty_1_r_SkillLogic();
			break;
			case "장미의 사수":
			passiveSkillLogic = new Kashyasty_2_r_SkillLogic();
			break;
			default:
			passiveSkillLogic = new BasePassiveSkillLogic();
			break;
		}

		passiveSkillLogic.passiveSkill = passiveSkill;

		return passiveSkillLogic;
	}
}
}
