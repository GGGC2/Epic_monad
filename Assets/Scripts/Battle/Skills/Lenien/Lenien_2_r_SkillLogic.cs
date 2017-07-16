using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Lenien_2_r_SkillLogic : BasePassiveSkillLogic {

	public override float ApplyIgnoreResistanceAbsoluteValueByEachPassive(Skill appliedSkill, Unit target, Unit caster, float resistance)
	{
		float ignoreAmountPerLevel = 0.6f;
		float baseAmountPerLevel = 42;

		TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();
		Tile tileUnderTarget = tileManager.GetTile(target.GetPosition());
		if (tileUnderTarget.GetTileElement() == Enums.Element.Water)
			resistance -= baseAmountPerLevel + (ignoreAmountPerLevel * GameData.PartyData.level);
		
		return resistance;
	}
}
}
