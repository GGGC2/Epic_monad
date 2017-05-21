using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Lenien_2_r_SkillLogic : BasePassiveSkillLogic {

	public override float ApplyIgnoreResistanceAbsoluteValueByEachPassive(SkillInstanceData skillInstanceData, float resistance)
	{
		Unit target = skillInstanceData.GetMainTarget();
		int partyLevel = MonoBehaviour.FindObjectOfType<BattleManager>().GetPartyLevel();
		float ignoreAmountPerLevel = 0.6f;
		float baseAmountPerLevel = 42;

		TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();
		Tile tileUnderTarget = tileManager.GetTile(target.GetPosition());
		if (tileUnderTarget.GetTileElement() == Enums.Element.Water)
			resistance -= baseAmountPerLevel + (ignoreAmountPerLevel * partyLevel);
		
		return resistance;
	}
}
}
