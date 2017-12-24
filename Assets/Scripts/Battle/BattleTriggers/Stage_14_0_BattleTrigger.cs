using System.Collections.Generic;
using System.Linq;
using Enums;

class Stage_14_0_BattleTrigger : BattleTrigger {
    // Stage 14에서 box01이 파괴되면 door05가, box02가 파괴되면 door06이,
    // box01, 02, 03이 모두 파괴되면 door01, 02, 03, 04, 05, 06가 모두 열리는 트리거
    public Stage_14_0_BattleTrigger(string data, TrigResultType resultType, StringParser commaParser) : base(data, resultType, commaParser) {
    }
    public override void Trigger() {
        Unit box = units[units.Count - 1];
        LogManager logManager = LogManager.Instance;
        List<Unit> doors = new List<Unit>();
        List<Unit> allUnits = UnitManager.Instance.GetAllUnits();
        switch(box.EngName) {
        case "box01":
            doors = allUnits.FindAll(unit => unit.EngName == "door05");
            break;
        case "box02":
            doors = allUnits.FindAll(unit => unit.EngName == "door06");
            break;
        }
        bool threeBoxesDestroyed = true;
        for(int i = 1; i <= 3; i++) {
            if(!units.Any(unit => unit.EngName == "box0" + i)) {
                threeBoxesDestroyed = false;
            }
        }
        if(threeBoxesDestroyed) {
            doors = allUnits.FindAll(unit => unit.EngName.Contains("door0"));
        }
        foreach(var door in doors) {
            logManager.Record(new UnitDestroyLog(door, TrigActionType.Kill));
        }
    }
}