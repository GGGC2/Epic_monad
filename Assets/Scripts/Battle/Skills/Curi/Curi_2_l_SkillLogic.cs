using System.Collections.Generic;
using Battle.Damage;
using Enums;

namespace Battle.Skills {
    public class Curi_2_1_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnActionEnd(Unit caster) {
            Tile currentTile = caster.GetTileUnderUnit();
            Element elementOfTile = currentTile.GetTileElement();
            List<StatusEffect> statusEffectList = caster.GetStatusEffectList();

            if(elementOfTile != Element.Fire) {
                statusEffectList = statusEffectList.FindAll(x => x.GetOriginSkillName() != "신속 반응");
                caster.SetStatusEffectList(statusEffectList);
            }
            else if(elementOfTile == Element.Fire) {
                StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
            }
        }
    }
}
