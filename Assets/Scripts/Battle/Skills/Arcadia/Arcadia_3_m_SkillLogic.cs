using System.Collections.Generic;
using Battle.Damage;
using Enums;

namespace Battle.Skills {
    class Arcadia_3_m_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnActionEnd(Unit caster) {
            Tile currentTile = caster.GetTileUnderUnit();
            Element elementOfTile = currentTile.GetTileElement();
            List<UnitStatusEffect> statusEffectList = caster.StatusEffectList;

            if (elementOfTile != Element.Plant) {
                UnitStatusEffect statusEffect = statusEffectList.Find(x => x.GetOriginSkillName() == "광합성");
                if (statusEffect != null)
                    caster.RemoveStatusEffect(statusEffect);
            } else StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
        }
    }
}
