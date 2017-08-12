using System.Collections;
using UnityEngine;
using Enums;

namespace Battle.Skills {
    public class Curi_6_m_SkillLogic : BaseSkillLogic {
        public override IEnumerator ActionInDamageRoutine(CastingApply castingApply) {
            BattleManager battleManager = MonoBehaviour.FindObjectOfType<BattleManager>();
            Unit target = castingApply.GetTarget();
            Unit caster = castingApply.GetCaster();
            float defense = target.GetStat(Stat.Defense);
            float resistance = target.GetStat(Stat.Resistance);
            if (target == caster) {
				yield return battleManager.StartCoroutine(target.ApplyDamageByNonCasting(target.GetMaxHealth() * 0.1f, caster, 
                                        -defense, -resistance, true, false, false));
            }
        }
    }
}
