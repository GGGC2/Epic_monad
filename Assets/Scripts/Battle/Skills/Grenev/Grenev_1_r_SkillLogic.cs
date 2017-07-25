using System.Collections;
using Battle.Damage;

namespace Battle.Skills {
    class Grenev_1_r_SkillLogic : BasePassiveSkillLogic {
        public override IEnumerator TriggerOnPhaseStart(Unit caster, int phase) {
            if(phase == 6)
                StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
            yield return null;
        }
    }
}
