using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Battle.Damage;

namespace Battle.Skills
{
public class Lenien_7_r_SkillLogic : BasePassiveSkillLogic {

	public override void TriggerActionEnd(Unit lenien)
	{
		StatusEffector.AttachStatusEffect(lenien, this.passiveSkill, lenien);
	} 

	public override void SetAmountToEachStatusEffect(List<StatusEffect> statusEffects, Unit lenien, Unit target) 
	{
		TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();
		List<Tile> nearbyTilesFromLenian = new List<Tile>();
		nearbyTilesFromLenian = tileManager.GetTilesInRange(Enums.RangeForm.Square, lenien.GetPosition(), 0, 1, lenien.GetDirection());

		float damageBonusPerTile = 0.1f;
		int numberOfMetalTiles = nearbyTilesFromLenian.Count(x => x.GetTileElement() == Enums.Element.Metal);
		// float totalPowerBonus = 1.0f + numberOfMetalTiles * damageBonusPerTile;

		statusEffects[0].SetRemainStack(numberOfMetalTiles);
		statusEffects[0].SetAmount(1 + damageBonusPerTile);
	}
}
}
