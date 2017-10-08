using UnityEngine;
namespace Battle.Skills {
    public class Curi_3_m_SkillLogic : BasePassiveSkillLogic {
        public override void ApplyBonusDamageFromEachPassive(CastingApply castingApply) {
            if (castingApply.GetSkill().GetName() == "수상한 덩어리")
                castingApply.GetDamage().relativeDamageBonus *= 0.6f;
        }
    }
}
