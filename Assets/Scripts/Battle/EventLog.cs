using System.Collections.Generic;
using UnityEngine;
using Enums;

public class EventLog : Log {
    List<EffectLog> effectLogList = new List<EffectLog>();
    public List<EffectLog> getEffectLogList() { return effectLogList; }
}

public class BattleStartLog : EventLog {

    public override string GetText() {
        return "전투 시작";
    }
}

public class MoveLog : EventLog {
    Unit unit;
    Vector2 beforePos;
    Direction beforeDirection;
    Vector2 afterPos;
    Direction afterDirection;

    public MoveLog(Unit unit, Vector2 beforePos, Direction beforeDirection,Vector2 afterPos,  Direction afterDirection) {
        this.unit = unit;
        this.beforePos = beforePos;
        this.beforeDirection = beforeDirection;
        this.afterPos = afterPos;
        this.afterDirection = afterDirection;
    }
    public override string GetText() {
        return unit.GetNameKor() + " : " + beforePos + "에서 " + afterPos + "(으)로 이동";
    }
}

public class ChainLog : EventLog {
    Casting casting;
    public ChainLog(Casting casting) {
        this.casting = casting;
    }
    public override string GetText() {
        Unit caster = casting.Caster;
        ActiveSkill activeSkill = casting.Skill;
        return caster.GetNameKor() + " : " + activeSkill.GetName() + " 연계 대기";
    }
}

public class CastLog : EventLog {
    Casting casting;
    public CastLog(Casting casting) {
        this.casting = casting;
    }
    public override string GetText() {
        Unit caster = casting.Caster;
        ActiveSkill activeSkill = casting.Skill;
        return caster.GetNameKor() + " : " + activeSkill.GetName() + " 사용";
    }
}

public class RestLog : EventLog {
    Unit rester;
    public RestLog(Unit rester) {
        this.rester = rester;
    }
    public override string GetText() {
        return rester.GetNameKor() + " 휴식";
    }
}

public class TurnStartLog : EventLog {
    Unit turnStarter;
    public TurnStartLog(Unit turnStarter) {
        this.turnStarter = turnStarter;
    }
    public override string GetText() {
        return turnStarter.GetNameKor() + "의 턴 시작";
    }
}

public class TurnEndLog : EventLog {
    Unit turnEnder;
    public TurnEndLog(Unit turnEnder) {
        this.turnEnder = turnEnder;
    }
    public override string GetText() {
        return turnEnder.GetNameKor() + "의 턴 끝";
    }
}
public class PhaseStartLog : EventLog {
    int phase;
    public PhaseStartLog(int phase) {
        this.phase = phase;
    }
    public override string GetText() {
        return "페이즈 " + phase + " 시작";
    }
}
public class PhaseEndLog : EventLog {
    int phase;
    public PhaseEndLog(int phase) {
        this.phase = phase;
    }
    public override string GetText() {
        return "페이즈 " + phase + " 끝";
    }
}
