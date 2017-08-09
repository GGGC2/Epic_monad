using Enums;

namespace Battle.Skills {
    public class Curi_3_r_SkillLogic : BasePassiveSkillLogic {
        public override bool TriggerStatusEffectAppliedToOwner(UnitStatusEffect statusEffect, Unit caster, Unit target) {
            if (target.GetTileUnderUnit().GetTileElement() == Element.Metal) {
                if (statusEffect.IsOfType(StatusEffectType.DefenseChange) || statusEffect.IsOfType(StatusEffectType.ResistanceChange))
                    return false;
            }
            return true;
        }
    }
}