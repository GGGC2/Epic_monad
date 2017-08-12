﻿using Battle.Damage;
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
        public override bool TriggerStatusEffectApplied(UnitStatusEffect statusEffect, Unit caster, Unit target, List<Tile> targetTiles) {
            if(caster == target) {
                UnitStatusEffect alreadyAppliedStatusEffect = caster.GetStatusEffectList().Find(se => se.GetOriginSkillName() == "떠밀기");
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
        public override bool TriggerStatusEffectRemoved(UnitStatusEffect statusEffect, Unit target) {
            int ap = (int)statusEffect.GetAmountOfType(StatusEffectType.Etc);
            skill.SetRequireAP(ap);
            return true;
        }
        public override IEnumerator ActionInDamageRoutine(SkillInstanceData skillInstanceData) {
            List<Tile> tiles = skillInstanceData.GetRealEffectRange();
            Unit caster = skillInstanceData.GetCaster();
            Unit target = skillInstanceData.GetTarget();

            StatusEffector.AttachStatusEffect(caster, skill, caster, tiles);
            Tile backTile = GetBackTile(caster, target);
            if(backTile != null && !backTile.IsUnitOnTile())
                target.ForceMove(backTile);

            yield return new WaitForSeconds(0.5f);
        }
    }
}
