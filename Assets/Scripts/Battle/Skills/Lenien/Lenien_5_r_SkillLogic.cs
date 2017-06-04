using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Battle.Damage;
using Enums;

namespace Battle.Skills
{
public class Lenien_5_r_SkillLogic : BasePassiveSkillLogic {

	public override void TriggerOnActionEnd(Unit lenien)
	{
		TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();
		
		if (lenien.GetTileUnderUnit().GetTileElement() == Element.Metal)
			StatusEffector.AttachStatusEffect(lenien, this.passiveSkill, lenien);
		else
		{
			List<StatusEffect> statusEffectList = lenien.GetStatusEffectList();
			statusEffectList = statusEffectList.FindAll(x => x.GetOriginSkillName() != "자기 부상");
			lenien.SetStatusEffectList(statusEffectList);
		}
	} 
}
}
