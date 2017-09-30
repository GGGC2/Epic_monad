using System.Collections;
using Enums;

namespace Battle.Skills {
    public class Luvericha_1_l_SkillLogic : BaseSkillLogic {
        public override void ApplyAdditionalDamage(CastingApply castingApply) {
            Unit caster = castingApply.GetCaster();
            Unit target = castingApply.GetTarget();
            castingApply.GetDamage().baseDamage = target.GetCurrentHealth() * 0.2f;
        }
    }
}
