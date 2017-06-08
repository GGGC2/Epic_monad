using System.Collections.Generic;
using System.Linq;

namespace Battle.Skills {
    class Eugene_2_r_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnRest(Unit caster) {
            caster.RemoveStatusEffect(Enums.StatusEffectCategory.Debuff, 1);
        }
    }
}
