
namespace Battle.Skills {
    class Luvericha_2_r_SkillLogic : BasePassiveSkillLogic {
        public override float ApplyAdditionalRecoverHealthDuringRest(Unit caster, float baseAmount) {
            return baseAmount * 1.5f;
        }
    }
}
