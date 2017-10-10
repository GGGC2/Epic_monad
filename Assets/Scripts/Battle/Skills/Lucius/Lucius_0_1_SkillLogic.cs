
using Enums;

namespace Battle.Skills {
    class Lucius_0_1_SkillLogic : BasePassiveSkillLogic {
        public override void TriggerOnUnitDestroy(Unit unit, Unit destroyedUnit, TrigActionType actionType) {
            if (unit.IsEnemy(destroyedUnit)) {
                foreach (var otherUnit in UnitManager.Instance.GetAllUnits())
                    if (unit.IsAlly(otherUnit))
                        if (actionType == TrigActionType.Kill)
                            otherUnit.ApplyDamageByNonCasting(10, unit, 0, 0, false, true, false);
                        else if (actionType == TrigActionType.Retreat)
                            otherUnit.RecoverActionPoint(10);
            }
        }
    }
}
