using Enums;
using UnityEngine;
using System.Collections.Generic;


public class EffectLog : Log {

}

public class HPChangeLog : EffectLog {
    Unit unit;
    int amount;
    public void setAmount(int amount) {
        this.amount = amount;
    }
    public HPChangeLog(Unit unit, int amount) {
        this.unit = unit;
        this.amount = amount;
    }
    public override string GetText() {
        string text;
        text = "\t" + unit.GetNameKor() + " : 체력 ";
        if (amount < 0) {
            text += -amount + " 감소";
        } else {
            text += amount + " 증가";
        }
        return text;
    }
}

public class APChangeLog : EffectLog {
    Unit unit;
    int amount;
    public void setAmount(int amount) {
        this.amount = amount;
    }
    public APChangeLog(Unit unit, int amount) {
        this.unit = unit;
        this.amount = amount;
    }
    public override string GetText() {
        string text;
        text = "\t" + unit.GetNameKor() + " : 행동력 ";
        if (amount < 0) {
            text += -amount + " 감소";
        } else { 
            text += amount + " 증가"; 
        }
        return text;
    }
}

public class CoolDownLog : EffectLog {
    Unit caster;
    string skillName;
    int skillCooldown;
    public CoolDownLog(Unit caster, string skillName, int skillCooldown) {
        this.caster = caster;
        this.skillName = skillName;
        this.skillCooldown = skillCooldown;
    }
    public override string GetText() {
        return "\t" + caster.GetNameKor() + " : " + skillName + " 재사용 대기 " + skillCooldown + "페이즈";
    }
}

public class DestroyUnitLog : EffectLog {
    Unit unit;
    public DestroyUnitLog(Unit unit) {
        this.unit = unit;
    }
    public override string GetText() {
        return "\t" + unit.GetNameKor() + " :  퇴각";
    }
}

public class RemoveChainLog : EffectLog {
    Unit unit;
    public RemoveChainLog(Unit unit) {
        this.unit = unit;
    }
    public override string GetText() {
        return "\t" + unit.GetNameKor() + " : 연계 해제";
    }
}

public class StatusEffectLog : EffectLog {
    StatusEffect statusEffect;  //typeof 를 통해 UnitStatusEffect인지 tileStatusEffect인지 알 수 있고,
                                //owner(또는 ownerTile) 변수를 통해 이 statusEffect를 가진 Object를 참조할 수 있음
    StatusEffectChangeType type;
    int index;
    float beforeAmount;
    float afterAmount;
    public void setAfterAmount(int afterAmount) {
        this.afterAmount = afterAmount;
    }
    public StatusEffectLog(StatusEffect statusEffect, StatusEffectChangeType type, int index, float beforeAmount, float afterAmount) {
        this.statusEffect = statusEffect;
        this.type = type;
        this.index = index;
        this.beforeAmount = beforeAmount;
        this.afterAmount = afterAmount;
    }
    public override string GetText() {
        string text = "\t";
        if (statusEffect is TileStatusEffect) {
            Tile ownerTile = ((TileStatusEffect)statusEffect).GetOwnerTile();
            text += "타일 " + ownerTile.GetTilePos();
        }
        else if (statusEffect is UnitStatusEffect) {
            Unit owner = ((UnitStatusEffect)statusEffect).GetOwner();
            text += owner.GetNameKor();
        }
        string name = statusEffect.GetDisplayName();
        text += " : " + name;
        if (type == StatusEffectChangeType.Remove)
            text += " 해제";
        else if (type == StatusEffectChangeType.Attach)
            text += " 부착";
        else {
            text += "의 ";
            if (type == StatusEffectChangeType.AmountChange)
                text += index + "번째 amount ";
            else if (type == StatusEffectChangeType.RemainAmountChange)
                text += index + "번째 remainAmount ";
            else if (type == StatusEffectChangeType.RemainPhaseChange)
                text += "남은 페이즈 ";
            else if (type == StatusEffectChangeType.RemainStackChange)
                text += name + "의 중첩 ";
            text += beforeAmount + " -> " + afterAmount;
        }
        return text;
    }
}

public class ForceMoveLog : EffectLog {
    Unit unit;
    Vector2 beforePos;
    Vector2 afterPos;

    public ForceMoveLog(Unit unit, Vector2 beforePos, Vector2 afterPos) {
        this.unit = unit;
        this.beforePos = beforePos;
        this.afterPos = afterPos;
    }
    public override string GetText() {
        return "\t" + unit.GetNameKor() + " : " + beforePos + "에서 " + afterPos + "로 강제이동";
    }
}

public class DirectionChangeLog : EffectLog {
    Unit unit;
    Direction beforeDirection;
    Direction afterDirection;

    public DirectionChangeLog(Unit unit, Direction beforeDirection, Direction afterDirection) {
        this.unit = unit;
        this.beforeDirection = beforeDirection;
        this.afterDirection =  afterDirection;
    }
    public override string GetText() {
        return "\t" + unit.GetNameKor() + " : " + beforeDirection + "에서 " + afterDirection + "(으)로 방향 변경";
    }
}

public class AISetActiveLog : EffectLog {
    Unit unit;

    public AISetActiveLog(Unit unit) {
        this.unit = unit;
    }
    public override string GetText() {
        return "\t" + unit.GetNameKor() + " : 활성화";
    }
}

public class CameraMoveLog : EffectLog {
    Vector2 position;

    public CameraMoveLog(Vector2 position) {
        this.position = position;
    }
    public override string GetText() {
        return "\t" + "카메라 위치 변경 : " + position;
    }
}