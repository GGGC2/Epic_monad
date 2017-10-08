using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace Battle.Skills {
    class Triana_1_r_SkillLogic : BaseSkillLogic {
        public override void ActionInDamageRoutine(CastingApply castingApply) {
            List<Tile> tiles = castingApply.GetRealEffectRange();
            Unit caster = castingApply.GetCaster();
            Unit target = castingApply.GetTarget();
            int reach = castingApply.GetSkill().GetFirstMaxReach();
            if (caster.element == Element.Plant) {
                Tile resultTile = Utility.GetGrabResultTile(caster, target);
                if (TileManager.Instance.isTilePassable(resultTile))
                    target.ForceMove(resultTile);
            }
        }
    }
}
