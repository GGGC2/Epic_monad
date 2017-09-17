using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Lenien_2_r_SkillLogic : BasePassiveSkillLogic {

	public override float ApplyIgnoreResistanceAbsoluteValueByEachPassive(ActiveSkill appliedSkill, Unit target, Unit caster, float resistance)
	{
		float ignoreAmountPerLevel = 0.7f;
		float baseAmountPerLevel = 51;

		TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();
		Tile tileUnderTarget = tileManager.GetTile(target.GetPosition());
		if (tileUnderTarget.GetTileElement() == Enums.Element.Water)
			resistance -= baseAmountPerLevel + (ignoreAmountPerLevel * GameData.PartyData.level);
		
		return resistance;
	}
}
}
