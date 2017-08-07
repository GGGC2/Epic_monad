﻿using System.Collections;
using Battle.Damage;

namespace Battle.Skills{
    class Grenev_2_r_SkillLogic : BasePassiveSkillLogic {
        public override IEnumerator TriggerOnKill(HitInfo hitInfo, Unit deadUnit) {
            StatusEffector.AttachStatusEffect(hitInfo.caster, passiveSkill, hitInfo.caster);
            yield return null;
        }
    }
}
