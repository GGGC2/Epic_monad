using UnityEngine;

namespace Battle.Skills {
    class Lucius_1_m_SkillLogic : BasePassiveSkillLogic {
        public override bool TriggerDamagedByCasting(Unit caster, Unit target, float damage) {
            Vector2 targetDirection = Utility.ToVector2(target.GetDirection());
            Vector2 displacement = caster.GetPosition() - target.GetPosition();
            return !(Vector2.Dot(targetDirection, displacement) == 0 && displacement.magnitude == 1);
        }
    }
}
