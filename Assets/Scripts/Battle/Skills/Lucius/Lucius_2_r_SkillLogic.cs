using UnityEngine;
using System.Collections;

namespace Battle.Skills {
    class Lucius_2_r_SkillLogic : BaseSkillLogic {
        public override void ActionInDamageRoutine(CastingApply castingApply) {
            Unit caster = castingApply.GetCaster();
            Unit target = castingApply.GetTarget();

            Tile resultTile = Utility.GetChargeResultTile(caster, target);
            caster.ForceMove(resultTile);
        }
    }
}
