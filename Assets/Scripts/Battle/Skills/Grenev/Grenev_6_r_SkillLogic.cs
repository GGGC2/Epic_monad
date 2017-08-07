using Battle.Damage;

namespace Battle.Skills {
    class Grenev_6_r_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnStart(Unit caster) {
            StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
        }
    }
}
