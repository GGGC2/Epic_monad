using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Battle.Damage;
using Enums;
using Battle.Skills;

namespace Battle.Skills {
    class Eugene_7_r_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnTurnStart(Unit caster, Unit turnStarter) {
            if(Utility.GetDistance(caster.GetPosition(), turnStarter.GetPosition()) <= 2) {
                StatusEffector.AttachStatusEffect(caster, passiveSkill, turnStarter);
            }
        }
    }
}
