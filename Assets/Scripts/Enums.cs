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
		Cross,
        DiagonalCross
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
        ContinuousHeal,
		Retire,
		DamageOverPhase,
		Exhaust,
		Bind,
		Silence,
		Faint,
		Slow,
		Wound,
        MaxHealthDecrease,
		PowerDecrease,
		DefenseDecrease,
		ResistanceDecrease,
        DexturityDecrease, 
		Mark,
		Poison,
		Bleed
	}
}
