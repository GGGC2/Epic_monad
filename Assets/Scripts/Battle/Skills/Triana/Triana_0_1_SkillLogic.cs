using Enums;

namespace Battle.Skills {
    class Triana_0_1_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnActionEnd(Unit caster) {
            Tile currentTile = caster.GetTileUnderUnit();
            Element elementOfTile = currentTile.GetTileElement();
            caster.myInfo.element = elementOfTile;
        }
    }
}
