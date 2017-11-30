using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;
using Battle.Skills;

namespace Battle.Turn{
	public class MoveStates{
		public static void MoveToTile(Vector2 destPos, Dictionary<Vector2, TileWithPath> path) {
            Unit unit = BattleData.turnUnit;
            List<Tile> destPath = path[destPos].path;
            Tile destTile = BattleData.tileManager.GetTile(destPos);

            int tileCount = 0;
            bool trapOperated = false;
            foreach(var tile in destPath) {
                tileCount++;
                List<TileStatusEffect> traps = TileManager.Instance.FindTrapsWhoseRangeContains(tile);
                foreach (var trap in traps) {
                    if (SkillLogicFactory.Get(unit.GetLearnedPassiveSkillList()).TriggerOnSteppingTrap(unit, trap)) {
                        Trap.OperateTrap(trap);
                        destTile = tile;
                        trapOperated = true;
                    }
                }
                if (trapOperated) break;
            }
            Direction finalDirection = Utility.GetFinalDirectionOfPath(destTile, destPath, BattleData.turnUnit.GetDirection());
            int totalAPCost = path[destTile.GetTilePos()].requireActivityPoint;

            unit.ApplyMove(destTile, finalDirection, totalAPCost, tileCount);
		}
	}
}
