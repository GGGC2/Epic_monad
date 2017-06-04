using UnityEngine;
using System.Linq;
using Enums;
using Battle.Damage;
using System.Collections.Generic;

namespace Battle.Skills {
    public class Lenien_6_l_SkillLogic : BasePassiveSkillLogic {

        public override void TriggerActiveSkillDamageApplied(Unit caster, Unit target) {
            int casterHeight = caster.GetHeight();
            int targetHeight = target.GetHeight();
            int deltaHeight = casterHeight - targetHeight;

            if (deltaHeight >= 2) {
                StatusEffector.AttachStatusEffect(caster, this.passiveSkill, target);
            }
        }
    }
}
