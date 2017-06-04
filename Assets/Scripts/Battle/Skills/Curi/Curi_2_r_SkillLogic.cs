using System.Collections.Generic;
using UnityEngine;
using Enums;

namespace Battle.Skills {
    public class Curi_2_r_SkillLogic : BaseSkillLogic {
        private List<Tile> TilesAroundTarget(Unit target) {
            TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();
            Vector2 targetPosition = target.GetPosition();
            List<Tile> tileList = tileManager.GetTilesInRange(RangeForm.Square, targetPosition, 0, 1, 0, Direction.Left);
            return tileList;
        }
        public override void ApplyAdditionalDamage(SkillInstanceData skillInstanceData) {
            List<Tile> tileList = TilesAroundTarget(skillInstanceData.GetMainTarget());

            int waterTileCount = 0;
            foreach(Tile tile in tileList) {
                if(tile.GetTileElement() == Element.Water)
                    waterTileCount++;
            }
            skillInstanceData.GetDamage().relativeDamageBonus *= (1 + waterTileCount * 0.1f);
        }
        public override bool TriggerStatusEffectApplied(StatusEffect statusEffect, Unit caster, Unit target) {
            List<Tile> tileList = TilesAroundTarget(target);
            bool isAllTileWater = true;
            foreach (Tile tile in tileList) {
                if (tile.GetTileElement() != Element.Water)
                    isAllTileWater = false;
            }
            if(!isAllTileWater) return false;
            return true;
        }
    }
}