using Battle.Damage;

namespace Battle.Skills {
    public class Curi_1_1_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerStart(Unit caster) {
            StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
        }
    }
}
