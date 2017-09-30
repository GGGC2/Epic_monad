using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace Battle.Skills {
    class Triana_1_r_SkillLogic : BaseSkillLogic {
        private Tile GetFrontTile(Unit caster, Unit target, int reach) {
            TileManager tileManager = BattleData.tileManager;
            Vector2 directionVector = Utility.ToVector2(caster.GetDirection());
            Vector2 currentPosition = target.GetPosition();
            for (int i = 0; i < reach; i++) {
                currentPosition -= directionVector;
                Tile tile = tileManager.GetTile(currentPosition);
                if (tile == null || tile.IsUnitOnTile())
                    return tileManager.GetTile(currentPosition + directionVector);
            }
            return tileManager.GetTile(currentPosition);
        }
        public override IEnumerator ActionInDamageRoutine(CastingApply castingApply) {
            List<Tile> tiles = castingApply.GetRealEffectRange();
            Unit caster = castingApply.GetCaster();
            Unit target = castingApply.GetTarget();
            int reach = castingApply.GetSkill().GetFirstMaxReach();
            if (caster.element == Element.Plant) {
                Tile frontTile = GetFrontTile(caster, target, reach);
                if (frontTile != null && !frontTile.IsUnitOnTile())
                    target.ForceMove(frontTile);
                yield return new WaitForSeconds(0.2f);
            }
            yield return null;
        }
    }
}
