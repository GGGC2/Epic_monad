using UnityEngine;
using System.Linq;
using Battle.Damage;
using System.Collections.Generic;

namespace Battle.Skills {
    public class Curi_1_1_SkillLogic : BasePassiveSkillLogic {

        public override void TriggerStart(Unit caster) {
            StatusEffector.AttachStatusEffect(caster, this.passiveSkill, caster);
        }
        public override void SetAmountToEachStatusEffect(List<StatusEffect> statusEffects, Unit caster, Unit target) {
            statusEffects[0].SetRemainStack(1);
            statusEffects[0].SetRemainPhase(5);
        }
    }
}
