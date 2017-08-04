using Battle.Damage;
using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;

namespace Battle.Skills {
    class Bianca_1_r_SkillLogic : BaseSkillLogic {
        private Tile GetBackTile(Unit caster, Unit target) {
            TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();
            Vector2 directionVector = target.GetPosition() - caster.GetPosition();
            Vector2 positionVector = target.GetPosition() + directionVector;
            return tileManager.GetTile(positionVector);
        }
        public override bool TriggerStatusEffectApplied(StatusEffect statusEffect, Unit caster, Unit target, List<Tile> targetTiles) {
            if(caster == target) {
                StatusEffect alreadyAppliedStatusEffect = caster.GetStatusEffectList().Find(se => se.GetOriginSkillName() == "떠밀기");
                int stack = 2;
                if (alreadyAppliedStatusEffect != null) {
                    stack += alreadyAppliedStatusEffect.GetRemainStack();
                }
                int apCoef = (int)statusEffect.GetAmountOfType(StatusEffectType.Etc);
                skill.SetRequireAP(apCoef * (stack * (stack + 1))/2);
                return true;
            }
            return false;
        }
        public override bool TriggerStatusEffectRemoved(StatusEffect statusEffect, Unit target) {
            int ap = (int)statusEffect.GetAmountOfType(StatusEffectType.Etc);
            skill.SetRequireAP(ap);
            return true;
        }
        public override IEnumerator ActionInDamageRoutine(SkillInstanceData skillInstanceData) {
            List<Tile> tiles = skillInstanceData.GetTiles();
            Unit caster = skillInstanceData.GetCaster();
            Unit target = skillInstanceData.GetMainTarget();

            StatusEffector.AttachStatusEffect(caster, skill, caster, tiles);
            Tile tileBefore = target.GetTileUnderUnit();
            Tile backTile = GetBackTile(caster, target);
            if(backTile != null && !backTile.IsUnitOnTile()) {
                tileBefore.SetUnitOnTile(null);
                target.transform.position = backTile.transform.position + new Vector3(0, 0, -0.05f);
                target.SetPosition(backTile.GetTilePos());
                backTile.SetUnitOnTile(target);
            }
            BattleTriggerChecker.CountBattleCondition(target, backTile);

            yield return new WaitForSeconds(0.5f);
        }
    }
}
