using UnityEngine;
using System.Collections;

namespace Enums {
	
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
		// 지정형, 범위형, 경로형.
		Point, Area, Route
	}
	
    public enum Stat
    {
        MaxHealth, 
        Power,
        Defense,
        Resistance,
        Dexturity
    }
    
	public enum RangeForm
	{
		Square,
		Straight,
		Diagonal,
		Cross,
        DiagonalCross,
		Sector
	}
	
	public enum SkillApplyType
	{
		Damage, Heal, Etc
	}
	
	public enum StatusEffectType
	{
        MaxHealthIncrease,
        PowerIncrease,
        DefenseIncrease,
        ResistanceIncrease, 
        DexturityIncrease,
        MaxHealthDecrease,
		PowerDecrease,
		DefenseDecrease,
		ResistanceDecrease,
        DexturityDecrease, 
        Smite, // 강타: 공격 시 추가 피해
        Silence, // 침묵: 기술 사용 불가
        RequireMoveAPIncrease, // 이동 시 행동력 소모 증가
        RequireMoveAPDecrease, // 이동 시 행동력 소모 감소 
        RequireSkillAPIncrease, // 기술 사용 시 행동력 소모 증가
        RequireSkillAPDecrease, // 기술 사용 시 행동력 소모 감소
        DamageIncrease, // 받는 피해 증가
        DamageDecrease, // 받는 피해 감소
        HealIncrease, // 받는 회복량 증가
        HealDecrease, // 받는 회복량 감소
        ContinuousDamage, // 지속 대미지: 페이즈가 시작될 때 대미지
        ContinuousHeal, // 지속 회복: 페이즈가 시작될 때 회복
		Bind, // 속박: 이동 불가
        Confused, // 혼란: 적과 아군을 반대로 인식
        Faint, // 기절: 턴이 돌아오면 자동 휴식
        Retire,
		Shield, 
        Etc // 위 분류에 해당하지 않는 효과
	}
}
