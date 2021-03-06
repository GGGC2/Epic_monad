﻿using Enums;

namespace Battle.Skills {
    class Eugene_4_m_SkillLogic : BaseSkillLogic{
        public override bool TriggerTileStatusEffectWhenUnitTryToChain(Tile tile, TileStatusEffect tileStatusEffect) {
            if(tile.GetUnitOnTile().GetUnitClass() == UnitClass.Magic) {
                return false;
            }
            return true;
        }
    }
}
