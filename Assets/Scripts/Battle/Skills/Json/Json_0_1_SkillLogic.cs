using System.Collections;
using Battle.Damage;
using Enums;
using UnityEngine;

namespace Battle.Skills {
    class Json_0_1_SkillLogic : BasePassiveSkillLogic {
        public override IEnumerator ActionInDamageRoutine(SkillInstanceData skillInstanceData) {
            Unit caster = skillInstanceData.GetCaster();
            Unit target = skillInstanceData.GetMainTarget();

            UnitStatusEffect alreadyAppliedStatusEffect = target.GetStatusEffectList().Find(se => se.GetOriginSkillName() == "표식");
            if(alreadyAppliedStatusEffect != null && alreadyAppliedStatusEffect.GetRemainStack() >= 4) {
                float lifeStealPercent = alreadyAppliedStatusEffect.GetAmountOfType(StatusEffectType.Etc);
                HitInfo hitInfo = target.GetLatelyHitInfos().Find(hi => hi.caster == caster);
                float recoverAmount = hitInfo.finalDamage * lifeStealPercent / 100f;

                BattleManager battleManager = MonoBehaviour.FindObjectOfType<BattleManager>();
                yield return battleManager.StartCoroutine(caster.RecoverHealth(recoverAmount));

                target.RemoveStatusEffect(alreadyAppliedStatusEffect);
            }
            else StatusEffector.AttachStatusEffect(caster, passiveSkill, target);
        }
    }
}
