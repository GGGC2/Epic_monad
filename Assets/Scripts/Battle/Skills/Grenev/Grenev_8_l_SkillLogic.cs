using System;
using System.Collections;
using UnityEngine;
using Enums;

namespace Battle.Skills {
    class Grenev_8_l_SkillLogic : BaseSkillLogic {
        private bool isHealthRatioSmallEnough(Unit target) {
            return (float)target.currentHealth / (float)target.GetMaxHealth() <= 0.35f;
        }
        public override bool MayDisPlayDamageCoroutine(CastingApply castingApply) {
            if (isHealthRatioSmallEnough(castingApply.GetTarget())) {
                return false;
            }
            return true;
        }
        public override IEnumerator ActionInDamageRoutine(CastingApply castingApply) {
            Unit caster = castingApply.GetCaster();
            Unit target = castingApply.GetTarget();
            if (isHealthRatioSmallEnough(target)) {
                BattleManager battleManager = MonoBehaviour.FindObjectOfType<BattleManager>();
                yield return battleManager.StartCoroutine(target.
					ApplyDamageByNonCasting(target.currentHealth, caster, target.GetStat(Stat.Defense), target.GetStat(Stat.Resistance), true, true, false));
            }
            else yield return null;
        }
        public override void ApplyAdditionalDamage(CastingApply castingApply) {
            if(isHealthRatioSmallEnough(castingApply.GetTarget())) {
                castingApply.GetDamage().relativeDamageBonus = 0;
            }
        }
        public override float ApplyIgnoreDefenceRelativeValueBySkill(float defense, Unit caster, Unit target) {
            if (isHealthRatioSmallEnough(target)) {
                return defense * 0.5f;
            }
            return defense;
        }
    }
}
