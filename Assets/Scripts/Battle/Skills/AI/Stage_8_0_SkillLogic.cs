using Battle.Damage;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Battle.Skills {
    class Stage_8_0_SkillLogic : BasePassiveSkillLogic {
        // 8스테이지 몬스터 패시브 "완강한 복족류" 스킬로직
        public override void TriggerOnStart(Unit caster) {
            StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
        }

        public override bool TriggerStatusEffectAppliedToOwner(UnitStatusEffect statusEffect, Unit caster, Unit target) {
            // "완강함"이 아닌 군중제어가 붙을 경우("완강함"도 군중 제어로 판정됨)
            if(statusEffect.IsCrowdControl() && statusEffect.GetOriginSkillName() != passiveSkill.GetName()) {
                UnitStatusEffect stubbornness = target.StatusEffectList.Find(se => se.GetOriginSkillName() == passiveSkill.GetName());
                target.RemoveStatusEffect(stubbornness);
            }
            return true;
        }
        public override bool TriggerStatusEffectRemoved(UnitStatusEffect statusEffect, Unit unit) {
            // 군중제어가 사라질 경우, 그리고 군중제어인 다른 cc기가 없을 경우
            if (statusEffect.IsCrowdControl() && statusEffect.GetOriginSkillName() != passiveSkill.GetName() &&
                !unit.StatusEffectList.Any(se => (se != statusEffect && se.IsCrowdControl()))) {
                StatusEffector.AttachStatusEffect(unit, passiveSkill, unit);
            }
            return true;
        }
    }
}
