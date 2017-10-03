using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace Battle.Skills {
    class Triana_1_r_SkillLogic : BaseSkillLogic {
        public override IEnumerator ActionInDamageRoutine(CastingApply castingApply) {
            List<Tile> tiles = castingApply.GetRealEffectRange();
            Unit caster = castingApply.GetCaster();
            Unit target = castingApply.GetTarget();
            int reach = castingApply.GetSkill().GetFirstMaxReach();
            if (caster.element == Element.Plant) {
                Tile resultTile = Utility.GetGrabResultTile(caster, target);
                if (resultTile != null && !resultTile.IsUnitOnTile())
                    target.ForceMove(resultTile);
                yield return new WaitForSeconds(0.2f);
            }
            yield return null;
        }
    }
}
