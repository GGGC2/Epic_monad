using System.Collections;
using Enums;

namespace Battle.Skills {
    public class Luvericha_1_l_SkillLogic : BaseSkillLogic {
        public override IEnumerator ActionInDamageRoutine(CastingApply castingApply) {
            Unit target = castingApply.GetTarget();
            Unit caster = castingApply.GetCaster();
            target.RemoveStatusEffect(caster, StatusEffectCategory.Buff, 1);
            yield return null;
        }
    }
}
