using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

namespace Battle.Skills {
    public class Curi_2_r_SkillLogic : BaseSkillLogic {
        public override void ApplyAdditionalDamage(SkillInstanceData skillInstanceData) {
            TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();
            Vector2 targetPosition = skillInstanceData.getTarget().GetPosition();
            Tile targetTile =  tileManager.GetTile(targetPosition).GetComponent<Tile>();

            List<GameObject> tileList = tileManager.GetTilesInRange(RangeForm.Diamond, targetPosition, 0, 1, Direction.Left);

            int waterTileCount = 0;

            foreach(GameObject tileAsGameObject in tileList) {
                Tile tile = tileAsGameObject.GetComponent<Tile>();
                if(tile.GetTileElement() == Enums.Element.Water) {
                    waterTileCount++;
                }
            }
            skillInstanceData.getDamage().relativeDamageBonus *= (1 + waterTileCount * 0.1f);
        }
    }
}