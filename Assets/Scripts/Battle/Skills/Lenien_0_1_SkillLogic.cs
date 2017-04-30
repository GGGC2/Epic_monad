using UnityEngine;
using System.Linq;
using Enums;
using Battle.Damage;
using System.Collections.Generic;

namespace Battle.Skills
{
public class Lenien_0_1_SkillLogic : BasePassiveSkillLogic {

	public override void TriggerActiveSkillDamageApplied(Unit caster, Unit target)
	{
		Tile tileUnderTarget = target.GetTileUnderUnit();

		if (tileUnderTarget.GetTileElement() == Element.Water || tileUnderTarget.GetTileElement() == Element.Metal)
		{
			StatusEffector.AttachStatusEffect(caster, this.passiveSkill, target);
		}
	}

	public override void SetAmountToEachStatusEffect(List<StatusEffect> statusEffects, Unit caster, Unit target)
	{
		int partyLevel = MonoBehaviour.FindObjectOfType<BattleManager>().GetPartyLevel();
		int amount = (int)(42 + partyLevel*0.6f);

		var statusEffect = statusEffects.Find(se => se.GetOriginSkillName() == "감전");
		statusEffect.SetAmount(0, amount);
	}
}
}
