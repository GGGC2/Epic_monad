using System.Collections.Generic;
using UnityEngine;
using Enums;
using System.Linq;

namespace Battle.Skills {
    public class Eren_1_l_SkillLogic : BaseSkillLogic {
        public override void ApplyAdditionalDamage(CastingApply castingApply) {
            UnitStatusEffect uniqueStatusEffect = castingApply.GetCaster().StatusEffectList.Find(se => se.GetDisplayName() == "í씉ì닔");

            if (uniqueStatusEffect != null) {
                int stack = uniqueStatusEffect.GetRemainStack();
                castingApply.GetDamage().baseDamage *= (1.0f + (0.2f * stack));
            }
        }
    }
}