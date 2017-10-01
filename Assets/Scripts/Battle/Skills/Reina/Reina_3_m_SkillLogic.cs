using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Battle.Damage;

namespace Battle.Skills
{
	public class Reina_3_m_SkillLogic : BasePassiveSkillLogic {
		public override void TriggerActiveSkillDamageApplied(Unit caster, Unit target){
			TileManager tileManager = BattleData.tileManager;
			if (tileManager.GetTile(target.GetPosition()).GetTileElement() == Enums.Element.Plant)
				StatusEffector.AttachStatusEffect(caster, this.passiveSkill, target);
		}
	}
}