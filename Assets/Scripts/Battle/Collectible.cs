using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Collectible {
    // 수집 가능한 오브젝트(collectible)에 대한 정보들
    public Unit unit;
    public int phase;
    public int range;

    public Collectible(List<string> data) {
        unit = UnitManager.Instance.units.Find(unit => unit.GetNameEng() == data[0]);
        phase = Int32.Parse(data[1]);
        range = Int32.Parse(data[2]);
    }
}