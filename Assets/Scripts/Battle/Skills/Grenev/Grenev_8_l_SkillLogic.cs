using System;
using System.Collections;
using UnityEngine;
using Enums;

namespace Battle.Skills {
    class Grenev_8_l_SkillLogic : BaseSkillLogic {
        private bool isHealthRatioSmallEnough(Unit target) {
            return (float)target.currentHealth / (float)target.GetMaxHealth() <= 0.35f;
        }
        public override bool MayDisPlayDamageCoroutine(SkillInstanceData skillInstanceData) {
            if (isHealthRatioSmallEnough(skillInstanceData.GetMainTarget())) {
                return false;
            }
            return true;
        }
        public override IEnumerator ActionInDamageRoutine(SkillInstanceData skillInstanceData) {
            Unit caster = skillInstanceData.GetCaster();
            Unit target = skillInstanceData.GetMainTarget();
            if (isHealthRatioSmallEnough(target)) {
                BattleManager battleManager = MonoBehaviour.FindObjectOfType<BattleManager>();
                yield return battleManager.StartCoroutine(target.
                        Damaged(target.currentHealth, caster, target.GetStat(Stat.Defense), target.GetStat(Stat.Resistance), true, true, false));
            }
            else yield return null;
        }
        public override void ApplyAdditionalDamage(SkillInstanceData skillInstanceData) {
            if(isHealthRatioSmallEnough(skillInstanceData.GetMainTarget())) {
                skillInstanceData.GetDamage().relativeDamageBonus = 0;
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
