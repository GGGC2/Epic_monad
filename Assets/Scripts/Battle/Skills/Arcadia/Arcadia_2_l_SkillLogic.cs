using Enums;
using System.Collections.Generic;
using Battle.Damage;

namespace Battle.Skills {
    class Arcadia_2_l_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnActionEnd(Unit caster) {
            Tile currentTile = caster.GetTileUnderUnit();
            Element elementOfTile = currentTile.GetTileElement();
            List<UnitStatusEffect> statusEffectList = caster.StatusEffectList;

            if (elementOfTile != Element.Plant) {
                UnitStatusEffect statusEffect = statusEffectList.Find(x => x.GetOriginSkillName() == "뿌리 내리기");
                if (statusEffect != null)
                    caster.RemoveStatusEffect(statusEffect);
            } else {
                StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
            }
        }
        public override bool TriggerOnForceMove(Unit caster, Tile tileAfter) {
            if(caster.GetTileUnderUnit().GetTileElement() == Element.Plant) {
                return false;
            }
            return true;
        }
    }
}
