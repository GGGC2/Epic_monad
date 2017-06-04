using UnityEngine;
using Battle.Damage;
using System.Collections.Generic;
using Enums;
using System.Linq;

namespace Battle.Skills
{
    public class Yeong_3_r_SkillLogic : BasePassiveSkillLogic
    {
	    public override void TriggerActionEnd(Unit yeong)
	    {
		    float percentRemainHealth = (float)yeong.GetCurrentHealth()/(float)yeong.GetMaxHealth();		
		    int stack = (int)(percentRemainHealth * 20);

		    if (stack == 0)
		    {
			    List<StatusEffect> statusEffectList = yeong.GetStatusEffectList();
			    statusEffectList = statusEffectList.FindAll(x => x.GetOriginSkillName() != "위험 돌파");
                yeong.SetStatusEffectList(statusEffectList);
		    }
		    else
		    {
			    StatusEffector.AttachStatusEffect(yeong, this.passiveSkill, yeong);
		    }
	    }
    }
}
