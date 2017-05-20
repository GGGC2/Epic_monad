using System.Collections.Generic;

namespace Battle.Skills {
    public class Curi_6_m_SkillLogic : BaseSkillLogic {
        public override void ActionInDamageRoutine(BattleData battleData, Skill appliedSkill, Unit unitInChain, Tile targetTile, List<Tile> selectedTiles) {
            unitInChain.currentHealth -= (int)(unitInChain.GetMaxHealth() * 0.1f);
        }
    }
}
