using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Battle.Skills {
    public class Curi_2_1_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerActionEnd(Unit caster) {
            Tile currentTile = caster.GetTile();
            Enums.Element elementOfTile = currentTile.GetTileElement();
            List<StatusEffect> statusEffectList = caster.GetStatusEffectList();
            StatusEffect statusEffectFromThisSkill = null;

            foreach(var statusEffect in statusEffectList) {
                if(statusEffect.fixedElem.display.displayName == "신속 반응") {
                    statusEffectFromThisSkill = statusEffect;
                }
            }

            if(elementOfTile != Enums.Element.Fire) {
                statusEffectFromThisSkill.SetToBeRemoved(true);
            }
            else if(elementOfTile == Enums.Element.Fire) {
                statusEffectFromThisSkill.SetToBeRemoved(false);
            }
        }
    }
}
