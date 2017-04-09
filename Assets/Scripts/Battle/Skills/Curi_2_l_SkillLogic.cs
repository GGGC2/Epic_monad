using UnityEngine;
using System.Linq;

namespace Battle.Skills {
    public class Curi_2_1_SkillLogic : BasePassiveSkillLogic {

        public override float GetAdditionalRelativePowerBonus(Unit caster) {
            TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();
            Tile casterTile = tileManager.GetTile(caster.GetPosition()).GetComponent<Tile>();

            if (casterTile.GetTileElement()== Enums.Element.Fire) {
                //TODO: 신속 효과가 추가될 시 수정
                return 1.2f;
            } 
            else return 1.0f;
        }
    }
}
