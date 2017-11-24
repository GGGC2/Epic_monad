using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Enums {
	public static class EnumUtil{
		public static List<Direction> directions;
		public static List<Direction> nonTileDirections;
		static EnumUtil(){
			directions = new List<Direction> ();
			directions.Add (Direction.LeftDown);
			directions.Add (Direction.RightDown);
			directions.Add (Direction.LeftUp);
			directions.Add (Direction.RightUp);
			nonTileDirections = new List<Direction> ();
			nonTileDirections.Add (Direction.Left);
			nonTileDirections.Add (Direction.Right);
			nonTileDirections.Add (Direction.Up);
			nonTileDirections.Add (Direction.Down);
		}
		public static IEnumerable<T> GetValues<T>(){
			return Enum.GetValues(typeof(T)).Cast<T>();
		}
	}
	public enum ConditionType{ Win, Lose, End, Bonus }
	public enum EffectVisualType{ Individual, Area, None }
	public enum EffectMoveType{ Move, NonMove, None }
	public enum Side{ Ally, Enemy, Neutral }
	// highlighting selected tile
	public enum TileColor{ Blue, Red, Yellow, Purple, Green, Black }
	public enum TileForm{ Flatland, Hill, Cliff, Water, HigherHill }
	public enum UnitClass{ None, Melee, Magic }
	public enum Celestial{ None, Sun, Moon, Earth }
	public enum Element{ None, Fire, Water, Plant, Metal }
    public enum Aspect { North, East, South, West } //임의로 지정해둔 방향. North가 기본 방향((0, 0)이 화면의 맨 왼쪽)임
	// 앞 4개만이 실제 게임의 타일 방향, 뒤 4개는 개발용 변수
	public enum Direction{ RightDown, RightUp, LeftUp, LeftDown, Right, Up, Left, Down }
	// 지정형, 경로형, 자동형, 재귀형
	public enum SkillType{ Point, Route, Auto, Self }
	//트리거 관련
	public enum TrigResultType{Win, Lose, Bonus, Trigger, Info}
	public enum TrigUnitType{Target, Ally, Enemy, NeutralChar, None, PC}
	public enum TrigActionType{Neutralize, Reach, Phase, Kill, Retreat, UnderCount, Rest, FriendShot, Cast, Effect, None}
    public enum ActionButtonType { Skill, Standby, Rest, Collect, Absent };
    public enum Stat
    {
        MaxHealth = 1,
        Power = 2,
        Defense = 3,
        Resistance = 4,
        Agility = 5,
        CurrentHealth = 6, // 엄밀히 말해 Stat은 아니지만 대미지/효과 계산에 포함되므로 추가
		UsedAP = 7, // CurrentHealth와 동일
		CurrentAP = 8, // CurrentHealth와 동일
        Level = 9,
		None = 10 // 대미지 없음, 고정값, 또는 기타 특수한 경우
    }

	public enum RangeForm
	{
		Front,
		AllDirection,
		Straight,
		Diamond,
		Square,
        Triangle,
		Cross,
        Diagonal,
		Sector,
		Global,
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

    public enum StatusEffectChangeType {
        Attach,
        Remove,
        AmountChange,
        RemainAmountChange,
        RemainPhaseChange,
        RemainStackChange
    }
	public enum StatusEffectType
	{
        MaxHealthChange,
        PowerChange,
        DefenseChange,
        ResistanceChange,
        AgilityChange,
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
		MeleeReflect,
		MagicReflect,
		Taunt, // 도발
		MeleeImmune, // 물리 면역
		MagicImmune, // 마법 면역
        Stealth,
		AllImmune,  // 모든 피해 면역
        Aura,       //오오라
        Trap,
        InactiveTrap,   // 활성화 대기 중인 덫
        Etc // 위 분류에 해당하지 않는 효과
	}

	public enum StatusEffectVar
	{
		None,
        Once,
		Stack, // 스택 비례일 경우
		Power,
        MaxHealth,
		Level,
		LostHpPercent, // 잃은 체력 %
		Etc // 기타 변수
	}
    public class EnumConverter {
        public static Stat GetCorrespondingStat(StatusEffectType statusEffectType) {
            switch (statusEffectType) {
            case StatusEffectType.PowerChange:
                return Stat.Power;
            case StatusEffectType.DefenseChange:
                return Stat.Defense;
            case StatusEffectType.ResistanceChange:
                return Stat.Resistance;
            case StatusEffectType.AgilityChange:
                return Stat.Agility;
            case StatusEffectType.MaxHealthChange:
                return Stat.MaxHealth;
            }
            return Stat.None;
        }
        public static StatusEffectType GetCorrespondingStatusEffectType(Stat stat) {
            switch (stat) {
            case Stat.Power:
                return StatusEffectType.PowerChange;
            case Stat.Defense:
                return StatusEffectType.DefenseChange;
            case Stat.Resistance:
                return StatusEffectType.ResistanceChange;
            case Stat.Agility:
                return StatusEffectType.AgilityChange;
            case Stat.MaxHealth:
                return StatusEffectType.MaxHealthChange;
            }
            return StatusEffectType.Etc;
        }
        public static StatusEffectType GetCorrespondingStatusEffectType(Element element) {
            switch (element) {
            case Element.Fire:
                return StatusEffectType.FireWeakness;
            case Element.Metal:
                return StatusEffectType.MetalWeakness;
            case Element.Plant:
                return StatusEffectType.PlantWeakness;
            case Element.Water:
                return StatusEffectType.WaterWeakness;
            }
            return StatusEffectType.Etc;
        }
    }
}
