using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Lenien_2_r_SkillLogic : BasePassiveSkillLogic {

	public override float ApplyIgnoreDefenceAbsoluteValueByEachPassive(float defense, Unit caster, Unit target)
	{
		int partyLevel = MonoBehaviour.FindObjectOfType<BattleManager>().GetPartyLevel();
		float ignoreAmountPerLevel = 0.6f;
		float baseAmountPerLevel = 42;

		TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();
		Tile tileUnderTarget = tileManager.GetTile(target.GetPosition()).GetComponent<Tile>();
		if (tileUnderTarget.GetTileElement() == Enums.Element.Water)
			defense -= baseAmountPerLevel + (ignoreAmountPerLevel * partyLevel);

		return defense;
	}
}
}
