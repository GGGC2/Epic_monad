using System.Collections;
using Enums;

namespace Battle.Skills {
    class Luvericha_6_l_SkillLogic : BaseSkillLogic {
        public override void ActionInDamageRoutine(CastingApply castingApply) {
            Unit target = castingApply.GetTarget();
            Unit caster = castingApply.GetCaster();
            target.RemoveStatusEffect(caster, StatusEffectCategory.Buff, 1);
        }
    }
}
