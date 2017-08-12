using System.Collections;
using Enums;

namespace Battle.Skills {
    public class Luvericha_1_l_SkillLogic : BaseSkillLogic {
        public override IEnumerator ActionInDamageRoutine(SkillInstanceData skillInstanceData) {
            Unit target = skillInstanceData.GetTarget();
            Unit caster = skillInstanceData.GetCaster();
            target.RemoveStatusEffect(caster, StatusEffectCategory.Buff, 1);
            yield return null;
        }
    }
}
