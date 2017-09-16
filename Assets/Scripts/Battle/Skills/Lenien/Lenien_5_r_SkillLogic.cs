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
			List<UnitStatusEffect> statusEffectList = lenien.StatusEffectList;
            UnitStatusEffect statusEffect = statusEffectList.Find(x => x.GetOriginSkillName() == "자기 부상");
            if(statusEffect != null)
                lenien.RemoveStatusEffect(statusEffect);
		}
	} 
}
}
