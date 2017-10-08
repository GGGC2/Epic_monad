using Battle.Damage;
using UnityEngine;
namespace Battle.Skills {
    public class Curi_1_1_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnStart(Unit caster) {
            StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
        }
    }
}
