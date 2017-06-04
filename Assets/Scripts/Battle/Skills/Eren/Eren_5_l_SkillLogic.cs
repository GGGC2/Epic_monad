using UnityEngine;
using System.Linq;
using Battle.Damage;
using System.Collections.Generic;

namespace Battle.Skills
{
    public class Eren_5_l_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerStart(Unit caster) {
            StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
        }
        public override float GetStatusEffectVar(StatusEffect statusEffect, int i, Unit unit) {
            UnitManager unitManager = MonoBehaviour.FindObjectOfType<UnitManager>();
            return unitManager.GetAllUnits().Count(x => x.GetSide() == Enums.Side.Enemy);
        }
    }
}
