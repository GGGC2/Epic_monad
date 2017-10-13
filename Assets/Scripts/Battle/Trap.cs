using System.Collections.Generic;
using System.Linq;
using Enums;
using UnityEngine;
using Battle.Damage;
using Battle.Skills;

class Trap {
    // 트랩 스킬로직에서 쓸 함수를 모아둔 모듈
    // 트랩 스킬의 스킬로직에서, TriggerTileStatusEffectOnApplied 메서드 내에서 TriggerOnApplied 메서드를 호출하고, (trap은 바로 적용시키지 않음)
    // TriggerTileStatusEffectAtPhaseStart 메서드 내에서 ActivateTrap을 호출하고,          (페이즈가 시작할 때 활성화)
    // TriggerTileStatusEffectsAtTurnStart 메서드 내에서 TriggerAtTurnStart를 호출해야 함. (턴이 시작할 때 위에 있는 유닛이 있으면 발동)

    public static bool TriggerOnApplied(TileStatusEffect tileStatusEffect) {
        if(!tileStatusEffect.IsOfType(StatusEffectType.Trap))
            return true;
        return false;
    }
    public static void ActivateTrap(TileStatusEffect inactiveTrap) { // 활성화
        Tile tile = inactiveTrap.GetOwnerTile();
        if(!inactiveTrap.IsOfType(StatusEffectType.InactiveTrap))
            return;
        Unit caster = inactiveTrap.GetCaster();
        Skill skill = inactiveTrap.GetOriginSkill();
        tile.RemoveStatusEffect(inactiveTrap);      // 활성화 대기 중이었던 tileStatusEffect를 제거
        List<TileStatusEffect> statusEffects = new List<TileStatusEffect>();
        if (skill is ActiveSkill) {
            statusEffects = ((ActiveSkill)skill).GetTileStatusEffectList()
                        .Select(fixedElem => new TileStatusEffect(fixedElem, caster, tile, skill))
                        .ToList();
        }
        List<TileStatusEffect> trapStatusEffects = statusEffects.FindAll(se => se.IsOfType(StatusEffectType.Trap));
        foreach (var trapStatusEffect in trapStatusEffects) {
            List<Unit> unitsInRange = GetUnitsInRange(trapStatusEffect);
            foreach(var unit in unitsInRange) {
                trapStatusEffect.GetMemorizedUnits().Add(unit);
            }
        }
        StatusEffector.AttachStatusEffect(caster, trapStatusEffects, tile);
    }
    public static void OperateTrap(TileStatusEffect trap) {  // 발동
        Tile tile = trap.GetOwnerTile();
        Unit caster = trap.GetCaster();
        List<Unit> unitsInRange = GetUnitsInRange(trap);
        foreach (var unit in unitsInRange) {
            Skill originSkill = trap.GetOriginSkill();
            if(originSkill is ActiveSkill)
                StatusEffector.AttachStatusEffect(caster, (ActiveSkill)originSkill, unit, GetTilesInRange(trap));
        }
		if (unitsInRange.Count > 0)
			tile.RemoveStatusEffect (trap);
		SoundManager.Instance.PlaySE ("OperateTrap");
    }
    public static void Update(TileStatusEffect trap) {
        Unit caster = trap.GetCaster();
        List<Unit> unitsInRange = GetUnitsInRange(trap);
        foreach(var unit in unitsInRange) {
            if (!trap.GetMemorizedUnits().Contains(unit)) {
                if (SkillLogicFactory.Get(unit.GetLearnedPassiveSkillList()).TriggerOnSteppingTrap(unit, trap)) {
                    LogManager.Instance.Record(new TrapOperatedLog(trap));
                    OperateTrap(trap);
                    return;
                }
            }
        }
    }
    public static void TriggerAtTurnStart(TileStatusEffect trap, Unit turnStarter) {
        if (trap.IsOfType(StatusEffectType.Trap))
            if (GetTilesInRange(trap).Contains(turnStarter.GetTileUnderUnit()))
                if (SkillLogicFactory.Get(turnStarter.GetLearnedPassiveSkillList()).TriggerOnSteppingTrap(turnStarter, trap)) {
                    LogManager.Instance.Record(new TrapOperatedLog(trap));
                    OperateTrap(trap);
                }
    }

    private static List<Tile> GetTilesInRange(TileStatusEffect trap) {
        Tile tile = trap.GetOwnerTile();
        TileManager tileManager = BattleData.tileManager;
        return tileManager.GetTilesInRange(RangeForm.Square, tile.position, 0,
                                    (int)trap.GetAmountOfType(StatusEffectType.Trap), 1, Direction.Down);
    }
    private static List<Unit> GetUnitsInRange(TileStatusEffect trap) {
        List<Unit> unitsInRange = new List<Unit>();
        List<Tile> trapRange = GetTilesInRange(trap);
        foreach (var tile in trapRange) {
            if (tile.IsUnitOnTile())
                unitsInRange.Add(tile.GetUnitOnTile());
        }
        return unitsInRange;
    }
}
