using UnityEngine;
using System.Collections.Generic;

namespace Battle.Skills {
    public class Curi_7_l_SkillLogic : BasePassiveSkillLogic {
        public override float GetAdditionalRelativePowerBonus(Unit caster) {
            TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();
            List<Tile> tileList = new List<Tile>();
            tileList.Add(caster.GetTileUnderUnit());
            int distance;
            for (distance=1; distance<=25; distance++) {
                bool unitFound = false;
                tileManager.AddNearbyTiles(tileList);
                foreach(Tile tile in tileList) {
                    if(tile.IsUnitOnTile() && tile.GetUnitOnTile() != caster) { 
                        unitFound = true;
                    }
                }
                if(unitFound)   break;
            }
            return 1 + (0.04f * distance);
        }
    }
}