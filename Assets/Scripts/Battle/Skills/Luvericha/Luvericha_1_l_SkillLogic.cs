using System.Collections;
using Enums;

namespace Battle.Skills {
    public class Luvericha_1_l_SkillLogic : BaseSkillLogic {
        public override IEnumerator ActionInDamageRoutine(SkillInstanceData skillInstanceData) {
            Unit target = skillInstanceData.GetMainTarget();
            target.RemoveStatusEffect(StatusEffectCategory.Buff, 1);
            yield return null;
        }
    }
}
