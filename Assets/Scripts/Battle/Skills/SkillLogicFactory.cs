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
        BaseSkillLogic skillLogic;
		switch (skill.GetName()) {
            // 그레네브
            case "저격":
            skillLogic = new Grenev_1_l_SkillLogic();
            break;
            case "단검술":
            skillLogic = new Grenev_1_m_SkillLogic();
            break;

            // 노엘
            case "파마의 섬광":
            skillLogic = new Noel_1_l_SkillLogic();
            break;
            case "신의 가호":
            skillLogic = new Noel_1_r_SkillLogic();
            break;

            // 달케니르
            case "에테르 미사일":
            skillLogic = new Darkenir_1_m_SkillLogic();
            break;

			// 레이나
			case "화염 폭발":
			skillLogic = new Reina_1_l_SkillLogic();
            break;
			case "화염구":
			skillLogic = new Reina_1_m_SkillLogic();
            break;
			// case "마력 연쇄":
			// return new Reina_3_r_SkillLogic();
			case "지옥 불꽃":
            skillLogic = new Reina_4_m_SkillLogic();
            break;
            // case "에테르 과부하":
            // return new Reina_5_r_SkillLogic();

            // 루베리카
            case "정화된 밤":
            skillLogic = new Luvericha_1_l_SkillLogic();
            break;
            case "사랑의 기쁨":
            skillLogic = new Luvericha_1_r_SkillLogic();
            break;
            case "에튀드 : 겨울바람":
            skillLogic = new Luvericha_4_l_SkillLogic();
            break;
            case "에튀드 : 햇빛":
            skillLogic = new Luvericha_4_r_SkillLogic();
            break;
            case "소녀의 기도":
            skillLogic = new Luvericha_5_m_SkillLogic();
            break;
            case "교향곡 : 운명":
            skillLogic = new Luvericha_6_l_SkillLogic();
            break;
            case "4분 33초":
            skillLogic = new Luvericha_8_l_SkillLogic();
            break;
            case "환상곡":
            skillLogic = new Luvericha_8_r_SkillLogic();
            break;

			// 리니안
			case "전자기 충격": case "전자기 충격_test":
            skillLogic = new Lenien_1_m_SkillLogic();
            break;
			case "하전 파동":
			skillLogic = new Lenien_2_m_SkillLogic();
			break;
			case "축전":
            skillLogic = new Lenien_7_l_SkillLogic();
            break;

            //비앙카
            case "잘근잘근 덫":
            skillLogic = new Bianca_1_l_SkillLogic();
            break;
            case "떠밀기":
            skillLogic = new Bianca_1_r_SkillLogic();
            break;

            //세피아
            case "반달베기":
            skillLogic = new Sepia_1_r_SkillLogic();
            break;

            //아르카디아
            case "갈고리 씨앗":
            skillLogic = new Arcadia_1_l_SkillLogic();
            break;

			// 영
			case "은빛 베기": case "은빛 베기_test":
            skillLogic = new Yeong_1_l_SkillLogic();
            break;
			case "섬광 찌르기":
            skillLogic = new Yeong_2_l_SkillLogic();
            break;
			case "초감각":
            skillLogic = new Yeong_5_r_SkillLogic();
            break;

			// 에렌
			case "칠흑의 화살":
            skillLogic = new Eren_1_l_SkillLogic();
            break;
			case "광휘": case "광휘_test":
            skillLogic = new Eren_1_r_SkillLogic();
            break;
			case "죽음의 화살": case "죽음의 화살_test":
            skillLogic = new Eren_3_l_SkillLogic();
            break;
			case "치유의 빛":
            skillLogic = new Eren_6_r_SkillLogic();
            break;

            //유진
            case "얼음 파편":
            skillLogic = new Eugene_1_l_SkillLogic();
            break;
            case "순백의 방패":
            skillLogic = new Eugene_1_m_SkillLogic();
            break;
            case "청명수의 축복":
            skillLogic = new Eugene_2_l_SkillLogic();
            break;
            case "거울 방패":
            skillLogic = new Eugene_3_m_SkillLogic();
            break;
            case "얼음의 가호":
            skillLogic = new Eugene_4_l_SkillLogic();
            break;
            case "백은의 장막":
            skillLogic = new Eugene_4_m_SkillLogic();
            break;
            case "겨울의 가호":
            skillLogic = new Eugene_8_l_SkillLogic();
            break;
            case "백은의 오로라":
            skillLogic = new Eugene_8_m_SkillLogic();
            break;

			// 카샤스티
			case "더블 샷":
            skillLogic = new Kashyasty_1_l_SkillLogic();
            break;

			// Not used
			/*case "조화진동":
            skillLogic = new HarmonySkillLogic();
            break;
			case "생명력 흡수":
            skillLogic = new LifeDrainSkillLogic();
            break;
			case "이매진 블릿":
            skillLogic = new ImagineBulletSkillLogic();
            break;*/
            
            //큐리
            case "수상한 덩어리":
            skillLogic = new Curi_1_m_SkillLogic();
            break;
            case "알칼리 폭탄":
            skillLogic = new Curi_2_r_SkillLogic();
            break;
            case "산성 혼합물":
            skillLogic = new Curi_4_m_SkillLogic();
            break;
            case "도금":
            skillLogic = new Curi_5_r_SkillLogic();
            break;
            case "유독성 촉매":
            skillLogic = new Curi_6_m_SkillLogic();
            break;
            case "에테르 폭탄":
            skillLogic = new Curi_7_r_SkillLogic();
            break;
            case "초강산 혼합물":
            skillLogic = new Curi_8_m_SkillLogic();
            break;
            default:
            skillLogic = new BaseSkillLogic();
            break;
		}
        skillLogic.skill = skill;
        return skillLogic;
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
            // 그레네브
            case "살의":
            passiveSkillLogic = new Grenev_1_r_SkillLogic();
            break;

            // 달케니르
            case "공허의 장벽":
            passiveSkillLogic = new Darkenir_1_l_SkillLogic();
            break;

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
			case "점화":
			passiveSkillLogic = new Reina_3_l_SkillLogic();
			break;
			case "잉걸불":
			passiveSkillLogic = new Reina_3_m_SkillLogic();
			break;
			case "뭉치면 죽는다":
			passiveSkillLogic = new Reina_5_l_SkillLogic();
			break;
			case "흩어져도 죽는다":
			passiveSkillLogic = new Reina_5_m_SkillLogic();
			break;
			// case "잿더미":
			// passiveSkillLogic = new Reina_6_l_SkillLogic();
			// break;
			// case "흐름 변환":
			// passiveSkillLogic = new Reina_6_r_SkillLogic();
			// break;
			case "열상 낙인":
			passiveSkillLogic = new Reina_7_m_SkillLogic();
			break;
            // case "에테르 순환":
            // passiveSkillLogic = new Reina_7_r_SkillLogic();
            // break;

            // 루베리카
            case "위기상황":
            passiveSkillLogic = new Luvericha_2_m_SkillLogic();
            break;
            case "자가치유":
            passiveSkillLogic = new Luvericha_2_r_SkillLogic();
            break;
            case "수호의 오오라":
            passiveSkillLogic = new Luvericha_3_m_SkillLogic();
            break;
            case "치유의 오오라":
            passiveSkillLogic = new Luvericha_3_r_SkillLogic();
            break;
            case "넘치는 사랑":
            passiveSkillLogic = new Luvericha_5_r_SkillLogic();
            break;
            case "영혼의 교감":
            passiveSkillLogic = new Luvericha_6_r_SkillLogic();
            break;
            case "헌신":
            passiveSkillLogic = new Luvericha_7_m_SkillLogic();
            break;
            case "응급처치":
            passiveSkillLogic = new Luvericha_7_r_SkillLogic();
            break;

			// 리니안
			case "감전":
			passiveSkillLogic = new Lenien_0_1_SkillLogic();
			break;
			case "에너지 변환":
			passiveSkillLogic = new Lenien_1_l_SkillLogic();
			break;
			case "전도체":
			passiveSkillLogic = new Lenien_2_r_SkillLogic();
			break;
			case "피뢰침":
			passiveSkillLogic = new Lenien_3_l_SkillLogic();
			break;
			case "정전기 유도":
			passiveSkillLogic = new Lenien_3_r_SkillLogic();
			break;
			case "연쇄 방전":
			passiveSkillLogic = new Lenien_5_m_SkillLogic();
			break;
			case "자기 부상":
			passiveSkillLogic = new Lenien_5_r_SkillLogic();
			break;
			case "에테르 전위":
			passiveSkillLogic = new Lenien_6_l_SkillLogic();
			break;
			// case "에테르 접지":
			// passiveSkillLogic = new Lenien_6_m_SkillLogic();
			// break;
			case "입자 가속":
			passiveSkillLogic = new Lenien_7_r_SkillLogic();
			break;

            //세피아
            case "신뢰의 끈":
            passiveSkillLogic = new Sepia_1_m_SkillLogic();
            break;

            //아르카디아
            case "광합성":
            passiveSkillLogic = new Arcadia_1_m_SkillLogic();
            break;

			// 영
			case "방랑자":
			passiveSkillLogic = new Yeong_0_1_SkillLogic();
			break;
			case "어려운 목표물":
			passiveSkillLogic = new Yeong_1_m_SkillLogic();
			break;
			case "동체 시력":
			passiveSkillLogic = new Yeong_1_r_SkillLogic();
			break;
			case "간파":
			passiveSkillLogic = new Yeong_2_m_SkillLogic();
			break;
			case "기척 감지":
			passiveSkillLogic = new Yeong_2_r_SkillLogic();
			break;
			case "질풍노도":
			passiveSkillLogic = new Yeong_3_m_SkillLogic();
			break;
			case "위험 돌파":
			passiveSkillLogic = new Yeong_3_r_SkillLogic();
			break;
			case "유법":
			passiveSkillLogic = new Yeong_5_m_SkillLogic();
			break;
			// case "접근":
			// passiveSkillLogic = new Yeong_6_l_SkillLogic();
			// break;
			case "무아지경":
			passiveSkillLogic = new Yeong_6_m_SkillLogic();
			break;
			case "끝없는 단련":
			passiveSkillLogic = new Yeong_6_r_SkillLogic();
			break;
			// case "후의 선":
			// passiveSkillLogic = new Yeong_7_m_SkillLogic();
			// break;
			case "명경지수":
			passiveSkillLogic = new Yeong_7_r_SkillLogic();
			break;

			// 에렌
			case "흡수":
			passiveSkillLogic = new Eren_0_1_SkillLogic();
			break;
			case "흑화":
			passiveSkillLogic = new Eren_2_l_SkillLogic();
			break;
			case "축성":
			passiveSkillLogic = new Eren_2_r_SkillLogic();
			break;
			case "초월":
			passiveSkillLogic = new Eren_3_m_SkillLogic();
			break;
			case "배척받는 자":
			passiveSkillLogic = new Eren_5_l_SkillLogic();
			break;
			case "영겁의 지식":
			passiveSkillLogic = new Eren_5_m_SkillLogic();
			break;
			case "진형 붕괴":
			passiveSkillLogic = new Eren_6_m_SkillLogic();
			break;
			case "압도적인 공포":
			passiveSkillLogic = new Eren_7_l_SkillLogic();
			break;
			case "천상의 전령":
			passiveSkillLogic = new Eren_7_l_SkillLogic();
			break;

            //유진
            case "색다른 휴식":
            passiveSkillLogic = new Eugene_2_r_SkillLogic();
            break;
            case "여행자의 발걸음":
            passiveSkillLogic = new Eugene_3_r_SkillLogic();
            break;
            case "순수한 물":
            passiveSkillLogic = new Eugene_5_l_SkillLogic();
            break;
            case "은의 지평선":
            passiveSkillLogic = new Eugene_5_m_SkillLogic();
            break;
            case "순은의 매듭":
            passiveSkillLogic = new Eugene_6_m_SkillLogic();
            break;
            case "야영 전문가":
            passiveSkillLogic = new Eugene_6_r_SkillLogic();
            break;
            case "청명수의 은총":
            passiveSkillLogic = new Eugene_7_l_SkillLogic();
            break;
            case "길잡이":
            passiveSkillLogic = new Eugene_7_r_SkillLogic();
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
            case "정제":
            passiveSkillLogic = new Curi_0_1_SkillLogic();
            break;
            case "호기심":
            passiveSkillLogic = new Curi_1_1_SkillLogic();
            break;
            case "신속 반응":
            passiveSkillLogic = new Curi_2_1_SkillLogic();
            break;
            case "가연성 부착물":
            passiveSkillLogic = new Curi_2_m_SkillLogic();
            break;
            case "조연성 부착물":
            passiveSkillLogic = new Curi_3_m_SkillLogic();
            break;
            case "동적 평형":
            passiveSkillLogic = new Curi_3_1_SkillLogic();
            break;
            case "환원":
            passiveSkillLogic = new Curi_3_r_SkillLogic();
            break;
            case "재결정":
            passiveSkillLogic = new Curi_5_l_SkillLogic();
            break;
            case "제한 구역":
            passiveSkillLogic = new Curi_7_l_SkillLogic();
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
