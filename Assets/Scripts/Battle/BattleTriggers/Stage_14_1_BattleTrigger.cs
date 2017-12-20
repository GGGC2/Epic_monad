using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Enums;

class Stage_14_1_BattleTrigger : BattleTrigger {
    // 홀수 phase가 시작될 땐 적이 (시작 위치 및 방향 기준) 왼쪽과 오른쪽 문에서, 짝수 phase가 시작될 땐 적이 앞쪽과 뒤쪽 문에서 등장함
    // 6개의 문이 모두 열렸다면 적이 모든 방향에서 등장함
    public Stage_14_1_BattleTrigger(string data, TrigResultType resultType, StringParser commaParser) : base(data, resultType, commaParser) {
    }
    public override void Trigger() {
        UnitManager unitManager = UnitManager.Instance;
        TileManager tileManager = TileManager.Instance;
        List<Unit> units = unitManager.GetAllUnits();
        bool allDoorsOpen = !units.Any(unit => unit.EngName.Contains("door"));

        Dictionary<Direction, List<Vector2>> generatePositionDict = new Dictionary<Direction, List<Vector2>>();
        generatePositionDict.Add(Direction.Left, new List<Vector2>());
        generatePositionDict.Add(Direction.Right, new List<Vector2>());
        generatePositionDict.Add(Direction.Up, new List<Vector2>());
        generatePositionDict.Add(Direction.Down, new List<Vector2>());
        foreach (var pos in tileManager.GetAllTiles().Keys) {
            if (pos.x == 1)   generatePositionDict[Direction.Left].Add(pos);
            if (pos.x == 31)  generatePositionDict[Direction.Right].Add(pos);
            if (pos.y == 1)   generatePositionDict[Direction.Down].Add(pos);
            if (pos.y == 34)  generatePositionDict[Direction.Up].Add(pos);
        }
        bool isOddPhase = (BattleData.currentPhase % 2 == 1);
        List<Vector2> positions = new List<Vector2>();
        List<Direction> directions = new List<Direction>();
        if(allDoorsOpen || isOddPhase) {
            positions.Add(generatePositionDict[Direction.Left][(int)UnityEngine.Random.Range(0, 4.99f)]);
            directions.Add(Direction.RightDown);
            positions.Add(generatePositionDict[Direction.Right][(int)UnityEngine.Random.Range(0, 4.99f)]);
            directions.Add(Direction.LeftUp);
        }
        if(allDoorsOpen || !isOddPhase) {
            positions.Add(generatePositionDict[Direction.Up][(int)UnityEngine.Random.Range(0, 4.99f)]);
            directions.Add(Direction.LeftDown);
            positions.Add(generatePositionDict[Direction.Down][(int)UnityEngine.Random.Range(0, 4.99f)]);
            directions.Add(Direction.RightUp);
        }
        unitManager.GenerateUnitsAtPosition(-1, positions, directions);
    }
}