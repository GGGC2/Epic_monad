using Battle.Damage;
using System.Collections;
using System.Collections.Generic;

namespace Battle.Skills {
    class Eugene_3_r_SkillLogic : BasePassiveSkillLogic{
        public override void TriggerOnMove(Unit caster) {
            if(!caster.GetHasUsedSkillThisTurn())
                StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
        }
    }
}
