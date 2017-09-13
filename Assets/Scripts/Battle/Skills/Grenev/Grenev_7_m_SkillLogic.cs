using Battle.Damage;
using System.Collections;

namespace Battle.Skills {
    class Grenev_7_m_SkillLogic : BasePassiveSkillLogic {
        public override IEnumerator TriggerOnPhaseStart(Unit caster, int phase) {
            if(caster.GetNotMovedTurnCount() >= 3) {
                StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
            }
            yield return null;
        }
        public override void TriggerOnMove(Unit caster) {
            UnitStatusEffect statusEffectToRemove = caster.StatusEffectList.Find(se => se.GetOriginSkillName() == passiveSkill.GetName());
            caster.RemoveStatusEffect(statusEffectToRemove);
        }
    }
}
