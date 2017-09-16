using System.Collections.Generic;

namespace Battle.Skills {
    class Eugene_3_m_SkillLogic : BaseSkillLogic {
        public override bool TriggerStatusEffectApplied(UnitStatusEffect statusEffect, Unit caster, Unit target, List<Tile> targetTiles) {
            List<UnitStatusEffect> statusEffectList = target.StatusEffectList;
            UnitStatusEffect statusEffectToChange = statusEffectList.Find(se => (se.GetOriginSkillName() == "순백의 방패"
                                        && se.IsOfType(Enums.StatusEffectType.Aura)));
            if(statusEffectToChange != null) {
                target.RemoveStatusEffect(statusEffectToChange);
                return true;
            }
            else return false;
        }
    }
}
