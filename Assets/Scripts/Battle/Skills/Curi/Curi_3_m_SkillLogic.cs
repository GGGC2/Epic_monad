
namespace Battle.Skills {
    public class Curi_3_m_SkillLogic : BasePassiveSkillLogic {
        public override void ApplyBonusDamageFromEachPassive(SkillInstanceData skillInstanceData) {
            if (skillInstanceData.GetSkill().GetName() == "수상한 덩어리")
                skillInstanceData.GetDamage().relativeDamageBonus *= 0.6f;
        }
    }
}
