using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

public class Casting {
	Unit caster;
	ActiveSkill skill;
	SkillLocation location;
	Tile pivotPos;

	public Casting(Unit caster, ActiveSkill skill, SkillLocation location){
		this.caster = caster;
		this.skill = skill;
		this.location = location;
		this.pivotPos = GetPivotPos(skill, location);
	}
	public Tile GetPivotPos(ActiveSkill skill, SkillLocation location) {
		Tile pivotTile;
		RangeForm rangeForm = skill.GetSecondRangeForm();

		if (rangeForm == RangeForm.Sector && skill.GetSecondMinReach() > 0) {
			Vector2 pos = location.TargetPos + Utility.ToVector2 (location.Direction);
			pivotTile = BattleData.tileManager.GetTile(pos);
		}
		else {
			pivotTile = location.TargetTile;
		}

		return pivotTile;
	}
	public Unit Caster{
		get { return caster; }
	}
	public ActiveSkill Skill{
		get { return skill; }
	}
	public Vector3 PivotPos{
		get { return pivotPos.realPosition; }
	}
	public SkillLocation Location{
		get { return location; }
	}
	public List<Tile> FirstRange{
		get { return Skill.GetTilesInFirstRange (Location); }
	}
	public List<Tile> SecondRange{
		get { return Skill.GetTilesInSecondRange (Location); }
	}
	public List<Tile> RealEffectRange{
		get { return Skill.GetTilesInRealEffectRange (Location); }
	}
    public List<Unit> Targets {
        get {
            List<Unit> targets = new List<Unit>();
            foreach(var tile in RealEffectRange) {
                if(tile.IsUnitOnTile())
                    targets.Add(tile.GetUnitOnTile());
            }
            return targets;
        }
    }
	public int RequireAP{
		get { return Caster.GetActualRequireSkillAP(Skill); }
	}
	public void Cast(int chainCombo){
		Skill.Apply(this, chainCombo);
	}
}