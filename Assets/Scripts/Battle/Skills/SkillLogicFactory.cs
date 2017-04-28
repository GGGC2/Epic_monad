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
			case "전자기 충격": case "전자기 충격_test":
			return new Lenien_1_m_SkillLogic();
			// 영
			case "은빛 베기": case "은빛 베기_test":
			return new Yeong_1_l_SkillLogic();
			// 에렌
			case "칠흑의 화살":
			return new Eren_1_l_SkillLogic();
			case "광휘": case "광휘_test":
			return new Eren_1_r_SkillLogic();
			case "죽음의 화살": case "죽음의 화살_test":
			return new Eren_3_l_SkillLogic();
			// 카샤스티
			case "더블 샷":
			return new Kashyasty_1_l_SkillLogic();

			case "조화진동":
			return new HarmonySkillLogic();
			case "생명력 흡수":
			return new LifeDrainSkillLogic();
			case "이매진 블릿":
			return new ImagineBulletSkillLogic();
            
            //큐리
            case "수상한 덩어리":
            return new Curi_1_m_SkillLogic();
            case "알칼리 폭탄":
            return new Curi_2_r_SkillLogic();
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

	public static BasePassiveSkillLogic Get(PassiveSkill passiveSkill)
	{
		BasePassiveSkillLogic passiveSkillLogic = null;
		switch (passiveSkill.GetName())
		{
			// 레이나
			case "핀토스의 긍지":
			passiveSkillLogic = new Reina_0_1_SkillLogic();
			break;
			case "상처 태우기":
			passiveSkillLogic = new Reina_2_l_SkillLogic();
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
			case "열상 낙인":
			passiveSkillLogic = new Reina_7_m_SkillLogic();
			break;
			// 리니안
			case "에너지 변환":
			passiveSkillLogic = new Lenien_1_l_SkillLogic();
			break;
			case "전도체":
			passiveSkillLogic = new Lenien_2_r_SkillLogic();
			break;
			case "연쇄 방전":
			passiveSkillLogic = new Lenien_5_m_SkillLogic();
			break;
			case "입자 가속":
			passiveSkillLogic = new Lenien_7_r_SkillLogic();
			break;
			// 영
			case "방랑자":
			passiveSkillLogic = new Yeong_0_1_SkillLogic();
			break;
			case "유법":
			passiveSkillLogic = new Yeong_1_m_SkillLogic();
			break;
			case "동체 시력":
			passiveSkillLogic = new Yeong_1_r_SkillLogic();
			break;
			case "기척 감지":
			passiveSkillLogic = new Yeong_2_r_SkillLogic();
			break;
			// 에렌
			case "흡수":
			passiveSkillLogic = new Eren_0_1_SkillLogic();
			break;
			case "초월":
			passiveSkillLogic = new Eren_3_m_SkillLogic();
			break;
			case "배척받는 자":
			passiveSkillLogic = new Eren_5_l_SkillLogic();
			break;
			case "진형 붕괴":
			passiveSkillLogic = new Eren_6_m_SkillLogic();
			break;
			case "압도적인 공포":
			passiveSkillLogic = new Eren_7_l_SkillLogic();
			break;
			// 트리아나
			case "나무 껍질":
			passiveSkillLogic = new Triana_2_r_SkillLogic();
			break;
			// 카샤스티
			case "장미 속의 가시":
			passiveSkillLogic = new Kashyasty_1_r_SkillLogic();
			break;
			case "장미의 사수":
			passiveSkillLogic = new Kashyasty_2_r_SkillLogic();
			break;
            // 큐리
            case "호기심":
            passiveSkillLogic = new Curi_1_1_SkillLogic();
            break;
            case "신속 반응":
            passiveSkillLogic = new Curi_2_1_SkillLogic();
            break;
            case "동적 평형":
            passiveSkillLogic = new Curi_3_1_SkillLogic();
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
