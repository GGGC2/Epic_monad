using UnityEngine;
using System.Linq;

namespace Battle.Skills {
    public class Curi_1_1_SkillLogic : BasePassiveSkillLogic {

        public override float GetAdditionalRelativePowerBonus(Unit caster) {
            if (MonoBehaviour.FindObjectOfType<BattleManager>().battleData.currentPhase <= 5) {
                return 1.15f;
            }
            else return 1.0f;
        }
    }
}
