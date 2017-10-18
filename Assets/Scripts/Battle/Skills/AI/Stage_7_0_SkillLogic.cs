using Battle.Damage;

namespace Battle.Skills {
    class Stage_7_0_SkillLogic : BasePassiveSkillLogic {
        // 7스테이지 요정 패시브 "생명의 갑옷" 스킬로직
        public override void TriggerOnStart(Unit caster) {
            StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
        }
    }
}
