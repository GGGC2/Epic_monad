using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace Battle.Skills {
    class Triana_1_l_SkillLogic : BaseSkillLogic {
        private Tile GetBackTile(Unit caster, Unit target) {
            TileManager tileManager = BattleData.tileManager;
            Vector2 directionVector = Utility.ToVector2(caster.GetDirection());
            Vector2 currentPosition = target.GetPosition();
            for (int i = 0; i < 3; i++) {
                currentPosition += directionVector;
                Tile tile = tileManager.GetTile(currentPosition);
                if (!tileManager.isTilePassable(tile))
                    return tileManager.GetTile(currentPosition - directionVector);
            }
            return tileManager.GetTile(currentPosition);
        }
        public override void ActionInDamageRoutine(CastingApply castingApply) {
            List<Tile> tiles = castingApply.GetRealEffectRange();
            Unit caster = castingApply.GetCaster();
            Unit target = castingApply.GetTarget();
            if (caster.myInfo.element == Element.Fire) {
                Tile backTile = GetBackTile(caster, target);
                if (TileManager.Instance.isTilePassable(backTile))
                    target.ForceMove(backTile);
            }
        }
    }
}
