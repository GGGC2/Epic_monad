using Battle.Damage;
using System.Linq;

namespace Battle.Skills {
    class Stage_14_0_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnPhaseStart(Unit caster, int phase) {
            UnitManager unitManager = UnitManager.Instance;
            
            if (unitManager.GetAllUnits().Any(unit => unit.GetNameEng().Contains("door0"))) {
                if ((phase - 1) % 3 == 0 && phase != 1) {
                    StatusEffector.AttachStatusEffect(caster, passiveSkill, caster);
                }
            }
        }
    }
}
