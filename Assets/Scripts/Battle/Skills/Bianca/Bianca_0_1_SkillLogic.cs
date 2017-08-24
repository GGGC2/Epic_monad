using UnityEngine;

namespace Battle.Skills {
    class Bianca_0_1_SkillLogic : BasePassiveSkillLogic {
        public override bool TriggerOnSteppingTrap(Unit caster, Tile tile, TileStatusEffect trap) {
            Debug.Log(caster.GetNameEng() + " " + trap.GetCaster().GetNameEng());
            if(trap.GetCaster() == caster) return false;
            return true;
        }
        public override bool TriggerDamaged(Unit target, float damage, Unit caster, bool isSourceTrap) {
            return !isSourceTrap;
        }
    }
}
