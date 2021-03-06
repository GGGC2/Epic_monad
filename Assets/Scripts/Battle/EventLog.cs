﻿using Enums;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//EffectLog와 달리 어떤 일이 발생했다는 사실만 기록.
public class EventLog : Log {
    public Unit actor;
    List<EffectLog> effectLogList = new List<EffectLog>();  // 이 Event로부터 발생한 Effect들
    public List<EffectLog> getEffectLogList() { return effectLogList; }

    public override IEnumerator Execute() {
        BattleTriggerManager TM = BattleTriggerManager.Instance;
        if(this is RestLog){
            TM.CountTriggers(TrigActionType.Rest, actor);
        }else if(this is CastLog){
            TM.CountTriggers(TrigActionType.Cast, actor);
            CastLog log = (CastLog)this;
            //(다른 진영 && 지형지물 아님)인 Target의 수
            int enemyTargetCount = log.casting.Targets.FindAll(unit => unit.GetSide() != actor.GetSide() && !unit.IsObject).Count;
            if(log.casting.Skill.IsChainable() && enemyTargetCount >= 2){
                TM.CountTriggers(TrigActionType.MultiAttack, actor, enemyTargetCount.ToString(), log: log);
            }
        }else if(this is MoveLog){
            MoveLog log = (MoveLog)this;
            TM.CountTriggers(TrigActionType.Escape, actor, dest: TileManager.Instance.GetTile(log.afterPos));
            TM.CountTriggers(TrigActionType.StepOnTile, actor, TileManager.Instance.GetTile(log.afterPos).displayName, log: log);
        }

        foreach (var effectLog in effectLogList) {
            if (!effectLog.executed) {
                effectLog.executed = true;
                if(this is CastLog && effectLog is DisplayDamageOrHealTextLog && 
                        !((CastLog)this).isLastTarget((DisplayDamageOrHealTextLog)effectLog)) {
                    BattleManager.Instance.StartCoroutine(effectLog.Execute());
                    yield return null;
                }
                else yield return effectLog.Execute();
            }
        }
    }
}

public class BattleStartLog : EventLog {
    public override string GetText() {
        return "전투 시작";
    }
}

public class MoveLog : EventLog {
    Vector2 beforePos;
    public Vector2 afterPos;

    public MoveLog(Unit unit, Vector2 beforePos, Vector2 afterPos) {
        this.actor = unit;
        this.beforePos = beforePos;
        this.afterPos = afterPos;
    }

    public override string GetText() {
        return actor.GetNameKor() + " : " + beforePos + "에서 " + afterPos + "(으)로 이동";
    }
}

public class MoveCancelLog : EventLog {
    BattleData.MoveSnapshot snapshot;

    public MoveCancelLog(Unit unit, BattleData.MoveSnapshot snapshot) {
        this.actor = unit;
        this.snapshot = snapshot;
    }
    public override string GetText() {
        return actor.GetNameKor() + " : 이동 취소";
    }
}

public class ChainLog : EventLog {  // 연계 대기
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

public class CastLog : EventLog {   // 스킬 사용
    public Casting casting;
    public CastLog(Casting casting) {
        this.actor = casting.Caster;
        this.casting = casting;
    }
    public override string GetText() {
        ActiveSkill activeSkill = casting.Skill;
        return actor.GetNameKor() + " : " + activeSkill.GetName() + " 사용";
    }
    public bool isLastTarget(DisplayDamageOrHealTextLog log) {
        List<Tile> realEffectRange = casting.RealEffectRange;
        List<Unit> targets = TileManager.GetUnitsOnTiles(realEffectRange);
        if(targets.Count == 0)  return false;
        return log.unit == targets.Last();
    }
}

public class CastByChainLog : EventLog {    // 연계 발동
    Casting casting;
    public CastByChainLog(Casting casting) {
        this.casting = casting;
    }
    public override string GetText() {
        Unit caster = casting.Caster;
        ActiveSkill activeSkill = casting.Skill;
        return caster.GetNameKor() + " : " + activeSkill.GetName() + " 연계 발동";
    }
    public bool isLastTarget(DisplayDamageOrHealTextLog log) {
        List<Tile> realEffectRange = casting.RealEffectRange;
        List<Unit> targets = TileManager.GetUnitsOnTiles(realEffectRange);
        if (targets.Count == 0) return false;
        return log.unit == targets.Last();
    }
}

public class RestLog : EventLog {
    public RestLog(Unit rester) {
        this.actor = rester;
    }
    public override string GetText() {
        return actor.GetNameKor() + " 휴식";
    }
}

public class StandbyLog : EventLog {
    Unit stander;
    public StandbyLog(Unit stander) {
        this.stander = stander;
    }
    public override string GetText() {
        return stander.GetNameKor() + "대기";
    }
}

public class CollectStartLog : EventLog {
    Unit collector;
    Unit collectible;
    public CollectStartLog(Unit collector, Unit collectible) {
        this.collector = collector;
        this.collectible = collectible;
    }
    public override string GetText() {
        return collector.GetNameKor() + " : " + collectible.GetNameKor() + " 수집 시작";
    }
}

public class TrapOperatedLog : EventLog {
    TileStatusEffect trap;

    public TrapOperatedLog(TileStatusEffect trap) {
        this.trap = trap;
    }
    public override string GetText() {
        return "타일" + trap.GetOwnerTile().GetTilePos() + " 에 있던 " + trap.GetDisplayName() + " 발동";
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
