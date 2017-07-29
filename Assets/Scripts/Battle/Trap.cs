﻿using System;
using System.Collections.Generic;
using System.Linq;
using Enums;
using UnityEngine;
using Battle.Damage;

class Trap {
    private static List<Tile> GetTilesInRange(TileStatusEffect trap, Tile trapTile) {
        TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();
        return tileManager.GetTilesInRange(RangeForm.Square, trapTile.position, 0,
                                    (int)trap.GetAmountOfType(StatusEffectType.Trap), 1, Direction.Down);
    }
    private static List<Unit> GetUnitsInRange(TileStatusEffect trap, Tile trapTile) {
        List<Unit> unitsInRange = new List<Unit>();
        List<Tile> trapRange = GetTilesInRange(trap, trapTile);
        foreach(var tile in trapRange) {
            if(tile.IsUnitOnTile())
                unitsInRange.Add(tile.GetUnitOnTile());
        }
        return unitsInRange;
    }
    public static bool TriggerOnApplied(TileStatusEffect tileStatusEffect, Tile tile) {
        if(!tileStatusEffect.IsOfType(StatusEffectType.Trap))
            return true;
        return false;
    }
    public static void ActivateTrap(TileStatusEffect trap, Tile tile) {
        Unit caster = trap.GetCaster();
        Skill skill = trap.GetOriginSkill();
        List<TileStatusEffect> statusEffects = skill.GetTileStatusEffectList()
                    .Select(fixedElem => new TileStatusEffect(fixedElem, caster, skill, null))
                    .ToList();
        List<TileStatusEffect> trapStatusEffects = statusEffects.FindAll(se => se.IsOfType(StatusEffectType.Trap));
        foreach (var trapStatusEffect in trapStatusEffects) {
            List<Unit> unitsInRange = GetUnitsInRange(trapStatusEffect, tile);
            foreach(var unit in unitsInRange) {
                trapStatusEffect.GetMemorizedUnits().Add(unit);
            }
        }
        StatusEffector.AttachStatusEffect(caster, trapStatusEffects, tile);
    }
    public static void OperateTrap(TileStatusEffect trap, Tile tile) {
        Unit caster = trap.GetCaster();
        List<Unit> unitsInRange = GetUnitsInRange(trap, tile);
        foreach (var unit in unitsInRange) {
            Skill originSkill = trap.GetOriginSkill();
            StatusEffector.AttachStatusEffect(caster, originSkill, unit, GetTilesInRange(trap, tile));
        }
        if (unitsInRange.Count > 0)
            tile.RemoveStatusEffect(trap);
    }
    public static void Update(TileStatusEffect trap, Tile tile) {
        Unit caster = trap.GetCaster();
        List<Unit> unitsInRange = GetUnitsInRange(trap, tile);
        foreach(var unit in unitsInRange) {
            if(!trap.GetMemorizedUnits().Contains(unit)) {
                OperateTrap(trap, tile);
                return;
            }
        }
    }
    public static void TriggerAtTurnStart(TileStatusEffect trap, Tile tile, Unit turnStarter) {
        if (trap.IsOfType(StatusEffectType.Trap))
            if (GetTilesInRange(trap, tile).Contains(turnStarter.GetTileUnderUnit())) {
            OperateTrap(trap, tile);
        }
    }
}