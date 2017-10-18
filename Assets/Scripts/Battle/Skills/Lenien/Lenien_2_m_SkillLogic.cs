using System.Collections.Generic;
using UnityEngine;

namespace Battle.Skills
{
public class Lenien_2_m_SkillLogic : BaseSkillLogic {

	// 조건부 기절 추가.
	public override bool TriggerStatusEffectApplied(UnitStatusEffect statusEffect, Unit caster, Unit target, List<Tile> targetTiles)
	{
		Tile midPointTile = targetTiles[0];

		if (GetDistance(target, midPointTile) > 1)
			return true;
		else
			return false;
	}

	int GetDistance(Unit target, Tile midPointTile)
	{
		int deltaX = (int)Mathf.Abs(target.GetPosition().x - midPointTile.GetTilePos().x);
		int deltaY = (int)Mathf.Abs(target.GetPosition().y - midPointTile.GetTilePos().y);
		
		return Mathf.Max(deltaX, deltaY);
	}
}
}