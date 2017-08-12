using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Battle.Skills {
    class Arcadia_1_r_SkillLogic : BaseSkillLogic {
        private Tile GetFrontTile(Unit caster, Unit target) {
            TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();
            Vector2 directionVector = target.GetPosition() - caster.GetPosition();
            directionVector.Normalize();
            Tile frontTile = caster.GetTileUnderUnit();
            while(frontTile == null || frontTile == caster.GetTileUnderUnit()) {
                frontTile = tileManager.GetTile(frontTile.position + directionVector);
            }
            return frontTile;
        }
        public override IEnumerator ActionInDamageRoutine(SkillInstanceData skillInstanceData) {
            Unit caster = skillInstanceData.GetCaster();
            Unit target = skillInstanceData.GetTarget();
            
            Tile frontTile = GetFrontTile(caster, target);
            target.ForceMove(frontTile);

            yield return new WaitForSeconds(0.5f);
        }
    }
}
