using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Battle.Skills {
    class Arcadia_1_r_SkillLogic : BaseSkillLogic {
        public override IEnumerator ActionInDamageRoutine(CastingApply castingApply) {
            Unit caster = castingApply.GetCaster();
            Unit target = castingApply.GetTarget();
            
            Tile resultTile = Utility.GetGrabResultTile(caster, target);
            target.ForceMove(resultTile);

            yield return new WaitForSeconds(0.2f);
        }
    }
}
