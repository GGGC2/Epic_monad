using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Enums;
using Battle.Damage;

namespace Battle.Skills
{
    public class Lenien_3_l_SkillLogic : BasePassiveSkillLogic
    {
        public override void TriggerOnPhaseStart(Unit caster, int phase)
        {
            int casterHeight = caster.GetHeight();

			UnitManager unitManager = BattleData.unitManager;
            List<Unit> allUnit = unitManager.GetAllUnits();
            bool isThereAnyUnitHigherThanCaster = allUnit.Any(x => x.GetHeight() > casterHeight);

			BattleManager battleManager = BattleData.battleManager;
            int currentPhase = BattleData.currentPhase;

            // 짝수 페이즈 시작 시, 자신보다 높은 유닛이 없으면 반경 2 영역 대상에게 기절 1페이즈
            if ((currentPhase % 2 == 0) && (!isThereAnyUnitHigherThanCaster))
            {
                BattleManager.MoveCameraToUnit(caster);
                Vector2 unitPosition = caster.GetPosition();
				TileManager tileManager = BattleData.tileManager;
                List<Tile> targetArea = tileManager.GetTilesInRange(RangeForm.Diamond, unitPosition, 1, 2, 0, Direction.LeftUp);
                List<Unit> targets = new List<Unit>();
                targetArea.ForEach(x =>
                {
                	if (x.IsUnitOnTile())
						targets.Add(x.GetUnitOnTile());
                });

                foreach (var target in targets)
                    StatusEffector.AttachStatusEffect(caster, this.passiveSkill, target);
            }
        }
    }
}