using System.Collections.Generic;
using Battle.Damage;
using Enums;

namespace Battle.Skills {
    class Arcadia_1_m_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnActionEnd(Unit caster) {
            Tile currentTile = caster.GetTileUnderUnit();
            Element elementOfTile = currentTile.GetTileElement();
            List<StatusEffect> statusEffectList = caster.GetStatusEffectList();

            if (elementOfTile != Element.Plant) {
                StatusEffect statusEffect = statusEffectList.Find(x => x.GetOriginSkillName() == "광합성");
                if (statusEffect != null)
                    caster.RemoveStatusEffect(statusEffect);
            } else {
                StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
            }
        }
    }
}
