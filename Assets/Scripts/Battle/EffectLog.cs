using Enums;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EffectLog : Log {
    public EventLog parentEvent;
    public virtual bool isMeaningless() {
        return false;
    }
}

public class HPChangeLog : EffectLog {
    public Unit unit;
    public int amount;
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
    public override IEnumerator Execute() {
        int maxHealth = unit.GetMaxHealth();
        int currentHealth = unit.GetCurrentHealth();
        int result = currentHealth + amount;

        if(result > maxHealth)  unit.currentHealth = maxHealth;
        else if(result < 0)     unit.currentHealth = 0;
        else                    unit.currentHealth = result;
        yield return null;
    }
    public override bool isMeaningless() {
        return amount == 0;
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
    public override IEnumerator Execute() {
        int currentAP = unit.activityPoint;
        int result = currentAP + amount;
        
        if (result < 0)     unit.activityPoint = 0;
        else                unit.activityPoint = result;

        UnitManager.Instance.UpdateUnitOrder();
        yield return null;
    }
    public override bool isMeaningless() {
        return amount == 0;
    }
}

/*public class StatChangeLog : EffectLog {
    Unit unit;
    Stat stat;
    int beforeAmount;
    int afterAmount;

    public StatChangeLog(Unit unit, Stat stat, int beforeAmount, int afterAmount) {
        this.unit = unit;
        this.stat = stat;
        this.beforeAmount = beforeAmount;
        this.afterAmount = afterAmount;
    }
    public override string GetText() {
        return "\t" + unit.GetNameKor() + stat + beforeAmount + " -> " + afterAmount; 
    }

    public override IEnumerator Execute() {
        unit.actualStats[stat].value = afterAmount;
        unit.updateCurrentHealthRelativeToMaxHealth();
        yield return null;
    }
}*/

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

    public override IEnumerator Execute() {
        caster.GetUsedSkillDict().Add(skillName, skillCooldown);
        yield return null;
    }
    public override bool isMeaningless() {
        return skillCooldown == 0;
    }
}

public class DestroyUnitLog : EffectLog {
    Unit unit;
    TrigActionType actionType;
    public DestroyUnitLog(Unit unit, TrigActionType actionType) {
        this.unit = unit;
        this.actionType = actionType;
    }
    public override string GetText() {
        string text = "\t" + unit.GetNameKor() + " : ";
        switch(actionType) {
        case TrigActionType.Kill:
            text += "죽음";
            break;
        case TrigActionType.Retreat:
            text += "퇴각";
            break;
        case TrigActionType.Reach:
            text += "목표지점 도달";
            break;
        }
        return text;
    }

    public override IEnumerator Execute() {
        BattleManager battleManager = BattleManager.Instance;
        yield return battleManager.StartCoroutine(BattleManager.DestroyUnit(unit, actionType));
    }
}
public class AddChainLog : EffectLog {
    Casting casting;

    public AddChainLog(Casting casting) {
        this.casting = casting;
    }
    public override string GetText() {
        Unit caster = casting.Caster;
        ActiveSkill skill = casting.Skill;
        return "\t" + caster.GetNameKor() + " : " + skill.GetName() + " 연계 대기열에 추가";
    }

    public override IEnumerator Execute() {
        Chain newChain = new Chain(casting);
        ChainList.GetChainList().Add(newChain);
        ChainList.SetChargeEffectToUnit(casting.Caster);
        yield return null;
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

    public override IEnumerator Execute() {
        List<Chain> chainList = ChainList.GetChainList();
        Chain chain = chainList.Find(x => x.Caster == unit);
        if (chain != null) {
            chainList.Remove(chain);
            ChainList.RemoveChargeEffectOfUnit(unit);
            unit.HideChainIcon();
        }
        yield return null;
    }
}

public class StatusEffectLog : EffectLog {
    public StatusEffect statusEffect;  //typeof 를 통해 UnitStatusEffect인지 tileStatusEffect인지 알 수 있고,
                                //owner(또는 ownerTile) 변수를 통해 이 statusEffect를 가진 Object를 참조할 수 있음
    public StatusEffectChangeType type;
    public int index;
    public float beforeAmount;
    public float afterAmount;
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
                text += " 중첩 ";
            text += beforeAmount + " -> " + afterAmount;
        }
        return text;
    }

    public override IEnumerator Execute() {
        if (type == StatusEffectChangeType.AmountChange || type == StatusEffectChangeType.RemainAmountChange
            || type == StatusEffectChangeType.RemainPhaseChange || type == StatusEffectChangeType.RemainStackChange) {
            switch (type) {
            case StatusEffectChangeType.AmountChange:
                statusEffect.flexibleElem.actuals[index].amount = afterAmount;
                break;
            case StatusEffectChangeType.RemainAmountChange:
                statusEffect.flexibleElem.actuals[index].remainAmount = afterAmount;
                break;
            case StatusEffectChangeType.RemainPhaseChange:
                statusEffect.flexibleElem.display.remainPhase = (int)afterAmount;
                break;
            case StatusEffectChangeType.RemainStackChange:
                int result;
                int maxStack = statusEffect.fixedElem.display.maxStack;
                result = (int)afterAmount;
                if (result > maxStack) result = maxStack;
                if (result < 0) result = 0;
                statusEffect.flexibleElem.display.remainStack = result;
                break;
            }
        } else {
            if (statusEffect is UnitStatusEffect) {
                UnitStatusEffect unitStatusEffect = (UnitStatusEffect)statusEffect;
                Unit owner = unitStatusEffect.GetOwner();
                switch (type) {
                case StatusEffectChangeType.Remove:
                    List<UnitStatusEffect> newStatusEffectList = owner.StatusEffectList.FindAll(se => se != statusEffect);
                    owner.SetStatusEffectList(newStatusEffectList);
                    break;
                case StatusEffectChangeType.Attach:
                    Debug.Log(owner.GetNameKor() + " " + unitStatusEffect.GetDisplayName() + " " + "부착");
                    owner.StatusEffectList.Add(unitStatusEffect);
                    break;
                }
            } else if (statusEffect is TileStatusEffect) {
                TileStatusEffect tileStatusEffect = (TileStatusEffect)statusEffect;
                Tile ownerTile = tileStatusEffect.GetOwnerTile();
                switch (type) {
                case StatusEffectChangeType.Remove:
                    List<TileStatusEffect> newStatusEffectList = ownerTile.StatusEffectList.FindAll(se => se != statusEffect);
                    ownerTile.SetStatusEffectList(newStatusEffectList);
                    break;
                case StatusEffectChangeType.Attach:
                    ownerTile.StatusEffectList.Add(tileStatusEffect);
                    break;
                }
            }
        }
        yield return null;
    }
    /*public override bool isValid() {  //statusEffect의 Validity 체크는 LogManager에서 함
        return !(type != StatusEffectChangeType.Attach && type != StatusEffectChangeType.Remove
                && beforeAmount == afterAmount);
    }*/
    public Unit GetOwner() {
        if(statusEffect is UnitStatusEffect)   return ((UnitStatusEffect)statusEffect).GetOwner();
        return null;
    }
    public float GetShieldChangeAmount() {
        if (statusEffect is UnitStatusEffect && statusEffect.IsOfType(StatusEffectType.Shield)) {
            Unit unit = ((UnitStatusEffect)statusEffect).GetOwner();
            float amount = afterAmount - beforeAmount;
            if (type == StatusEffectChangeType.RemainAmountChange) {
                return amount;
            } else if (type == StatusEffectChangeType.Attach)
                return statusEffect.GetAmountOfType(StatusEffectType.Shield);
            else if (type == StatusEffectChangeType.Remove)
                return -statusEffect.GetRemainAmountOfType(StatusEffectType.Shield);
        }
        return 0;
    }
}

public class PositionChangeLog : EffectLog {
    public Unit unit;
    Vector2 beforePos;
    public Vector2 afterPos;

    public PositionChangeLog(Unit unit, Vector2 beforePos, Vector2 afterPos) {
        this.unit = unit;
        this.beforePos = beforePos;
        this.afterPos = afterPos;
    }
    public override string GetText() {
        return "\t" + unit.GetNameKor() + " : " + beforePos + "에서 " + afterPos + "(으)로 위치 변경";
    }
    public override IEnumerator Execute() {
        TileManager tileManager = TileManager.Instance;
        Tile destTile = tileManager.GetTile(afterPos);
        unit.GetTileUnderUnit().SetUnitOnTile(null);
        unit.transform.position = destTile.transform.position + new Vector3(0, 0, -0.05f);
        unit.SetPosition(destTile.GetTilePos());
        destTile.SetUnitOnTile(unit);
        unit.notMovedTurnCount = 0;
        yield return null;
    }
    public override bool isMeaningless() {
        return beforePos == afterPos;
    }
}
	
public class PaintTilesLog : EffectLog {
	List<Tile> tiles;
	TileColor color;
	public PaintTilesLog(List<Tile> tiles, TileColor color) {
		this.tiles = tiles;
		this.color = color;
	}
	public override IEnumerator Execute() {
		BattleData.tileManager.PaintTiles(tiles, color);
		yield return null;
	}
	public override bool isMeaningless() {
		return tiles == null || tiles.Count == 0;
	}
}	

public class DepaintTilesLog : EffectLog {
	TileColor color;
	public DepaintTilesLog(TileColor color) {
		this.color = color;
	}
	public override IEnumerator Execute() {
		BattleData.tileManager.DepaintAllTiles(color);
		yield return null;
	}
	public override bool isMeaningless() {
		return false;
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
    public override IEnumerator Execute() {
        unit.GetComponent<AIData>().isActive = true;
        yield return null;
    }
    public override bool isMeaningless() {
        return unit.GetComponent<AIData>().isActive;
    }
}

public class CameraMoveLog : EffectLog {
	MonoBehaviour obj;

	public CameraMoveLog(MonoBehaviour obj) {
		this.obj = obj;
	}
    public override string GetText() {
		return "\t" + "카메라 위치 변경 : " + obj.name;
    }
    public override IEnumerator Execute() {
		if (!isMeaningless ()) {
			Vector2 objPos = (Vector2)obj.gameObject.transform.position;
			yield return BattleManager.SlideCameraToPosition (objPos);
			yield return null;
		}
	}
	public override bool isMeaningless() {
		if (obj == null) {
			return true;
		}
		Vector2 cameraPos = (Vector2)Camera.main.transform.position;
		Vector2 objPos = (Vector2)obj.gameObject.transform.position;
		return cameraPos.x == objPos.x && cameraPos.y == objPos.y;
    }
}

public class PrintBonusTextLog : EffectLog {
    string type;
    float amount;
    bool activate;

    public PrintBonusTextLog(string type, float amount, bool activate) {
        this.type = type;
        this.amount = amount;
        this.activate = activate;
    }

    public override string GetText() {
        if(activate)
            return "\t" + "추가데미지 패널 활성화 : " + type + "(x" + amount + ")";
        else
            return "\t" + "추가데미지 패널 비활성화 : " + type;
    }

    public override IEnumerator Execute() {
        UIManager uiManager = UIManager.Instance;
        switch(type) {
        case "DirectionBack":
            uiManager.PrintDirectionBonus(DirectionCategory.Back, amount);
            break;
        case "DirectionSide":
            uiManager.PrintDirectionBonus(DirectionCategory.Back, amount);
            break;
        case "Celestial":
            uiManager.PrintCelestialBonus(amount);
            break;
        case "Chain":
            if(activate) uiManager.PrintChainBonus((int)amount);
            else uiManager.chainBonusObj.SetActive(false);
            break;
        case "Height":
            uiManager.PrintHeightBonus(amount);
            break;
        case "All":
            if(!activate)
                uiManager.DeactivateAllBonusText();
            break;
        }
        yield return null;
    }
}

public class SoundEffectLog : EffectLog {
    ActiveSkill skill;

    public SoundEffectLog(ActiveSkill skill) {
        this.skill = skill;
    }
    public override string GetText() {
        return "\t" + "음향 효과 : " + skill.GetName();
    }
    public override IEnumerator Execute() {
        skill.ApplySoundEffect();
        yield return null;
    }
}

public class VisualEffectLog : EffectLog {
    Casting casting;

    public VisualEffectLog(Casting casting) {
        this.casting = casting;
    }
    public override string GetText() {
        return "\t" + "시각 효과 : " + casting.Skill.GetName();
    }
    public override IEnumerator Execute() {
        yield return BattleManager.Instance.StartCoroutine(casting.Skill.ApplyVisualEffect(casting));
    }
}

public class DisplayDamageOrHealTextLog : EffectLog {
    public Unit unit;
    int amount;
    bool isHealth;

    public DisplayDamageOrHealTextLog(Unit unit, int amount, bool isHealth) {
        this.unit = unit;
        this.amount = amount;
        this.isHealth = isHealth;
    }
    public override string GetText() {
        return "\t" + unit.GetNameKor() + " : 회복 텍스트 표시(" + amount + ")";
    }
    public override IEnumerator Execute() {
        if(amount <= 0)          yield return unit.DisplayDamageText(-amount, isHealth);
        else if(amount > 0)    yield return unit.DisplayRecoverText(amount, isHealth);
    }
}

public class AddLatelyHitInfoLog : EffectLog {
    Unit unit;
    HitInfo hitInfo;

    public AddLatelyHitInfoLog(Unit unit, HitInfo hitInfo) {
        this.unit = unit;
        this.hitInfo = hitInfo;
    }
    public override string GetText() {
        return "\t" + unit.GetNameKor() + " : latelyHitInfo 추가 (" + hitInfo.caster.GetNameKor() +", " + hitInfo.skill + ", " + hitInfo.finalDamage;
    }
    public override IEnumerator Execute() {
        unit.GetLatelyHitInfos().Add(hitInfo);
        yield return null;
    }
    public override bool isMeaningless() {
        return hitInfo == null;
    }
}

public class WaitForSecondsLog : EffectLog {
    float second;

    public WaitForSecondsLog(float second) {
        this.second = second;
    }
    public override string GetText() {
        return "\t" + "WaitForSeconds(" + second + ")";
    }

    public override IEnumerator Execute() {
        yield return new WaitForSeconds(second);
    }
}