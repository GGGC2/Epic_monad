using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace Battle.Skills
{
public class Reina_3_m_SkillLogic : BasePassiveSkillLogic {

	public override void SetAmountToEachStatusEffect(List<StatusEffect> statusEffects, Unit caster, Unit target)	
	{
		TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();
		float powerFactor = 1.3f;
		float amount = caster.GetActualStat(Enums.Stat.Power) * powerFactor;
		
		// 10 * 갯수 %
		var statusEffect1st = statusEffects.Find(se => se.GetOriginSkillName() == "내상");
		if (tileManager.GetTile(target.GetPosition()).GetTileElement() != Enums.Element.Plant)
			statusEffects.Remove(statusEffect1st);
		else
		{
			statusEffect1st.SetRemainPhase(2);
			statusEffect1st.SetAmount(0, amount);
		}
	}
}
}