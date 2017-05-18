using System.Collections.Generic;
using Battle.Damage;

namespace Battle.Skills {
    public class Curi_2_1_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerActionEnd(Unit caster) {
            Tile currentTile = caster.GetTileUnderUnit();
            Enums.Element elementOfTile = currentTile.GetTileElement();
            List<StatusEffect> statusEffectList = caster.GetStatusEffectList();

            if(elementOfTile != Enums.Element.Fire) {
                statusEffectList = statusEffectList.FindAll(x => x.GetOriginSkillName() != "신속 반응");
                caster.SetStatusEffectList(statusEffectList);
            }
            else if(elementOfTile == Enums.Element.Fire) {
                StatusEffector.AttachStatusEffect(caster, this.passiveSkill, caster);
            }
        }
    }
}
