using Enums;

namespace Battle.Skills {
    class Eugene_8_m_SkillLogic : BaseSkillLogic {
        public override bool TriggerTileStatusEffectWhenUnitTryToUseSkill(Tile tile, TileStatusEffect tileStatusEffect) {
            if (tile.GetUnitOnTile().GetUnitClass() == UnitClass.Magic) {
                return false;
            }
            return true;
        }
        public override bool TriggerTileStatusEffectWhenStatusEffectAppliedToUnit(CastingApply castingApply, Tile tile, TileStatusEffect tileStatusEffect) {
            Unit caster = castingApply.GetCaster();
            if(caster.GetUnitClass() == UnitClass.Magic) {
                return false;
            }
            return true;
        }
    }
}
