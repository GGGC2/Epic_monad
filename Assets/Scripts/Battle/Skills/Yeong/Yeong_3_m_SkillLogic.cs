using UnityEngine;
using Enums;
using Battle.Damage;
using System.Collections.Generic;

namespace Battle.Skills
{
    public class Yeong_3_m_SkillLogic : BasePassiveSkillLogic {

	    public override void TriggerEvasionEvent(BattleData battleData, Unit caster, Unit yeong)
	    {
		    StatusEffector.AttachStatusEffect(yeong, this.passiveSkill, yeong);
	    }
    }
}
