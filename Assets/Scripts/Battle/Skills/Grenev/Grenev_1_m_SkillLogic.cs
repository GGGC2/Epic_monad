using UnityEngine;
using System.Collections.Generic;
using Enums;
using System.Collections;

namespace Battle.Skills {
    class Grenev_1_m_SkillLogic : BaseSkillLogic {
		public override bool CheckApplyPossibleToTargetTiles(Unit caster, List<Tile> targetTiles) {
			TileManager tileManager = BattleData.tileManager;
			foreach(Tile tile in targetTiles){
				if(tile.IsUnitOnTile()) {
					Unit target = tile.GetUnitOnTile();
					Vector2 targetDirection = Utility.ToVector2(target.GetDirection());
					Vector2 skillDirection = (target.GetPosition() - caster.GetPosition()).normalized;
					if(targetDirection == skillDirection) {
						return true;
					}
				}
			}
            return false;
        }
        public override bool IgnoreShield(CastingApply castingApply) {
            return true;
        }
        public override float ApplyIgnoreDefenceAbsoluteValueBySkill(float defense, Unit caster, Unit target) {
			return System.Math.Min(0, defense);
        }
    }
}
