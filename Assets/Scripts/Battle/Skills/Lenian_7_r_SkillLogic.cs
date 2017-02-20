using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Battle.Skills
{
public class Lenian_7_r_SkillLogic : BasePassiveSkillLogic {

	public override float GetAdditionalPowerBonus(Unit caster)
	{
		TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();
		List<GameObject> nearbyTilesFromLenian = new List<GameObject>();
		nearbyTilesFromLenian = tileManager.GetTilesInRange(Enums.RangeForm.Square, caster.GetPosition(), 0, 1, caster.GetDirection());

		float damageBonusPerTile = 0.1f;
		int numberOfMetalTiles = nearbyTilesFromLenian.Count(x => x.GetComponent<Tile>().GetTileElement() == Enums.Element.Metal);
		float totalPowerBonus = 1.0f + numberOfMetalTiles * damageBonusPerTile;

		return totalPowerBonus;
	}
}
}
