using UnityEngine;
using System.Collections;

namespace Enums {
	
	public enum BattleResult
	{
		Win, Lose
	}

	public enum EffectVisualType
	{
		Individual, Area
	}
	
	public enum EffectMoveType
	{
		Move, NonMove
	}
	
	public enum Side
	{
		Ally, Enemy
	}
	
	public enum TileColor // highlighting selected tile
	{
		Blue, Red, Yellow
	}
	
	public enum TileForm
	{
		Flatland, Hill
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
		Square,
		Straight,
		Diagonal,
		Cross,
        DiagonalCross,
		Sector, 
		Global,
		Auto,
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
	
	public enum StatusEffectType
	{
        MaxHealthChange,
        PowerChange,
        DefenseChange,
        ResistanceChange, 
        DexturityChange,
        Smite, // 강타: 공격 시 추가 피해
        Silence, // 침묵: 기술 사용 불가
        RequireMoveAPChange, // 이동 시 행동력 소모 증감
        RequireSkillAPChange, // 기술 사용 시 행동력 소모 증감
        DamageChange, // 받는 피해 증감
        HealChange, // 받는 회복량 증감
        ContinuousDamage, // 지속 대미지
		ContinuousHeal, // 지속 힐
		Bind, // 속박: 이동 불가
        Confused, // 혼란: 적과 아군을 반대로 인식
        Faint, // 기절: 턴이 돌아오면 자동 휴식
        Retire,
		Shield,
		ConditionalShield, // 조건부 보호막: 특정 조건 만족 시 보호막 발동 
		Reflect, // 반사: 받는 피해의 일부만큼 공격자에게 피해
        Etc // 위 분류에 해당하지 않는 효과
	}
}
