using UnityEngine;
using System.Collections;

namespace Enums {

	public enum BattleResult
	{
		Win, Lose, End
	}

	public enum EffectVisualType
	{
		Individual, Area, None
	}

	public enum EffectMoveType
	{
		Move, NonMove, None
	}

	public enum Side
	{
		Ally, Enemy, Neutral
	}

	public enum TileColor // highlighting selected tile
	{
		Blue, Red, Yellow
	}

	public enum TileForm
	{
		Flatland, Hill, Cliff, Water, HigherHill
	}

	public enum UnitClass
	{
		None, Melee, Magic
	}

	public enum Celestial
	{
		None, Sun, Moon, Earth
	}

	public enum Element
	{
		None, Fire, Water, Plant, Metal
	}

	public enum Direction
	{
		// 위 4개만이 실제 게임의 타일 방향, 아래 4개는 개발용 변수
		LeftUp,
        LeftDown,
        RightUp,
        RightDown,
        Left,
        Right,
        Up,
        Down
	}

	public enum SkillType
	{
		// 지정형, 경로형, 자동형, 재귀형
		Point, Route, Auto, Self
	}

    public enum Stat
    {
        MaxHealth,
		CurrentHealth, // 엄밀히 말해 Stat은 아니지만 대미지/효과 계산에 포함되므로 추가
        Power,
        Defense,
        Resistance,
        Dexturity,
		UsedAP, // CurrentHealth와 동일
		CurrentAP, // CurrentHealth와 동일
		None // 대미지 없음, 고정값, 또는 기타 특수한 경우
    }

	public enum RangeForm
	{
		Diamond,
		Square,
		Straight,
		Diagonal,
		Cross,
        DiagonalCross,
		AllDirection,
		Sector,
		Global,
		Auto,
		Front,
		Self
	}

	public enum SkillApplyType
	{
		// 스킬 타입의 효과 우선 순위: Tile > Damage > Heal > Debuff > Buff > Move
		DamageHealth,
		DamageAP,
		HealHealth,
		HealAP,
		Buff, // 체인 불가능한 버프 효과 스킬
		Debuff, // 체인 가능한 디버프 효과 스킬
		Move,
		Tile,
		Etc
	}

	public enum StatusEffectCategory
	{
		Buff,
		Debuff,
		All
	}

	public enum DirectionCategory
	{
		Front,
		Side,
		Back
	}

	public enum StatusEffectType
	{
        PowerChange,
        DefenseChange,
        ResistanceChange,
        DexturityChange,
		SpeedChange, // 속도 증감
		UsedAPChange,
		CurrentAPChange,
		EvasionChange,
        Smite, // 강타: 공격 시 추가 피해
        Silence, // 침묵: 기술 사용 불가
        RequireMoveAPChange, // 이동 시 행동력 소모 증감
        RequireSkillAPChange, // 기술 사용 시 행동력 소모 증감
        DamageChange, // 주는 데미지 증감
		TakenDamageChange, // 받는 데미지 증감
		FireWeakness, // 받는 속성피해 증가
		WaterWeakness,
		PlantWeakness,
		MetalWeakness,
		HealChange, // 주는 회복량 증감
        TakenHealChange, // 받는 회복량 증감
		DamageOverPhase, // 지속 대미지
		HealOverPhase, // 지속 힐
		Bind, // 속박: 이동 불가
        Confused, // 혼란: 적과 아군을 반대로 인식
        Faint, // 기절: 턴이 돌아오면 자동 휴식
        Retire,
		Shield,
		ConditionalShield, // 조건부 보호막: 특정 조건 만족 시 보호막 발동
		Reflect, // 반사: 받는 피해의 일부만큼 공격자에게 피해
		Taunt, // 도발
		MeleeImmune, // 물리 면역
		MagicImmune, // 마법 면역
        Etc // 위 분류에 해당하지 않는 효과
	}

	public enum StatusEffectVar
	{
		None,
		Power,
		BuffFromOther, // 레이나 : 다른유닛에게서 받은 강화효과 수
		Level,
		MetalTile, // 리니안 : 주변 사각 1타일 금속타일 수
		NearbyEnemy, // 영 : 주변 반경 2타일 내 적의 수
		NearestUnit, // 큐리 : 가장 가까운 유닛으로부터의 거리
		LostHpPercent, // 잃은 체력 %
		CurrentHp, // 현재 체력
		RemainEnemy, // 에렌 : 남은 적의 수
		Absorption, // 에렌 : 흡수 중첩 수
		Absorption_1r, // 에렌 : 광휘-강타 연산 (0.6(+흡수 중첩당 0.1)x공격력)
		DamagedAlly, // 루베리카 : 남은 체력 40% 이하인 아군 수		
		Etc // 기타 변수
	}
}
