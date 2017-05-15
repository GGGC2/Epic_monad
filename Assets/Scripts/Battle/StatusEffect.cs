using Enums;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class StatusEffect {
    public FixedElement fixedElem;
    public FlexibleElement flexibleElem;

    // 공유목록
    public class FixedElement {

        public readonly DisplayElement display;
        public readonly List<ActualElement> actuals;
        public class DisplayElement
		{
            public readonly string originSkillName; // 효과를 불러오는 기술의 이름 
            public readonly string displayName; // 유저에게 보일 이름
            public readonly bool isBuff; // 버프일 경우 true
            public readonly bool isInfinite; // 페이즈 지속 제한이 없을 경우 true
            public readonly bool isStackable; // 상태 이상 중첩이 가능한 경우 true
            public readonly bool isOnce; // 다음 1회의 행동에만 적용되는 경우 true (예: 강타)
			public readonly int defaultPhase; // 일반적인 경우 상태이상이 지속되는 페이즈
            public readonly StatusEffectVar stackVar; // 한번에 쌓이는 스택 수
            public readonly int maxStack; // 최대 가능한 스택 수
            public readonly bool isRemovable; // 다른 기술에 의해 해제 가능할 경우 true

            // 이펙트 관련 정보
            public readonly string effectName;
            public readonly EffectVisualType effectVisualType;
            public readonly EffectMoveType effectMoveType;

            public DisplayElement(string originSkillName, string displayName,  
                  bool isBuff, bool isInfinite, 
                  bool isStackable, bool isOnce,
                  int defaultPhase, StatusEffectVar stackVar, int maxStack, bool isRemovable, 
                  string effectName, EffectVisualType effectVisualType, EffectMoveType effectMoveType)
            {
                this.originSkillName = originSkillName;
                this.displayName = displayName;
                this.isBuff = isBuff;
                this.isInfinite = isInfinite;
                this.isStackable = isStackable;
                this.isOnce = isOnce;
				this.defaultPhase = defaultPhase;
                this.stackVar = stackVar;
                this.maxStack = maxStack;
                this.isRemovable = isRemovable;
                this.effectName = effectName;
                this.effectVisualType = effectVisualType;
                this.effectMoveType = effectMoveType;
            }
        }

		public class ActualElement
		{
            public readonly StatusEffectType statusEffectType; // 시스템 상으로 구분하는 상태이상의 종류 

            // var * coef + base
            public readonly StatusEffectVar seVar;
            public readonly float seCoef;
            public readonly float seBase;

            public readonly bool isMultifly;

            public ActualElement(StatusEffectType statusEffectType, 
                                 StatusEffectVar statusEffectVar, float statusEffectCoef, float statusEffectBase, 
                                 bool isMultifly)
            {
                this.statusEffectType = statusEffectType;
                this.seVar = statusEffectVar;
                this.seCoef = statusEffectCoef;
                this.seBase = statusEffectBase;
				this.isMultifly = isMultifly;
            }
		}

		public FixedElement(string originSkillName, string displayName, 
                  bool isBuff, bool isInfinite, 
                  bool isStackable, bool isOnce,
                  int defaultPhase, StatusEffectVar stackVar, int maxStack, bool isRemovable,
                  string effectName, EffectVisualType effectVisualType, EffectMoveType effectMoveType, List<ActualElement> actualEffects)
		{
			display = new DisplayElement(originSkillName, displayName,
					isBuff, isInfinite,
					isStackable, isOnce,
                    defaultPhase, stackVar, maxStack, isRemovable,
					effectName, effectVisualType, effectMoveType);

			actuals = actualEffects;
		}
	}
        
	public class FlexibleElement {

        public DisplayElement display;
        public List<ActualElement> actuals;
        public class DisplayElement
		{
			public Unit caster; // 시전자
			public int remainStack; // 지속 단위가 적용 횟수 단위인 경우 사용
			public int remainPhase; // 지속 단위가 페이즈 단위인 경우 사용
			
            public DisplayElement(Unit caster, StatusEffectVar remainStack, int remainPhase)
            {
                this.caster = caster;
                this.remainStack = (int)GetSEVar(remainStack, caster);
                this.remainPhase = remainPhase;
            }
		}

		public class ActualElement
		{
            public float amount; // 영향을 주는 실제 값
            public float remainAmount; // 남은 수치 (실드 등)

            public ActualElement(int stack, FixedElement.ActualElement aeInFixed, Unit caster)
            {
                this.amount = CalculateAmount(stack, aeInFixed, caster);
                this.remainAmount = amount; // 초기화
            }
		}

		public FlexibleElement(FixedElement fixedElem, Unit caster)
		{
			int maxStack = fixedElem.display.maxStack;
            StatusEffectVar stackVar = fixedElem.display.stackVar;
            int stack = (int)GetSEVar(stackVar, caster);
			int defaultPhase = fixedElem.display.defaultPhase;
			display = new DisplayElement(caster, stackVar, defaultPhase);

			List<ActualElement> actuals = new List<ActualElement>();
			for (int i = 0; i < fixedElem.actuals.Count; i++)
            {
                actuals.Add(new ActualElement(stack, fixedElem.actuals[i], caster));
            }
            this.actuals = actuals;
		}
	}
	
	public StatusEffect(FixedElement fixedElem, Unit caster)
	{
		this.fixedElem = fixedElem;
		this.flexibleElem = new FlexibleElement(fixedElem, caster);
    }
	
    public string GetOriginSkillName() {return fixedElem.display.originSkillName;}
    public string GetDisplayName() {return fixedElem.display.displayName;}
    public bool GetIsBuff() {return fixedElem.display.isBuff;}
    public bool GetIsInfinite() {return fixedElem.display.isInfinite;}
    public bool GetIsStackable() {return fixedElem.display.isStackable;}
    public bool GetIsOnce() {return fixedElem.display.isOnce;}
    public bool GetIsRemovable() {return fixedElem.display.isRemovable;}
    public string GetEffectName() {return fixedElem.display.effectName;}
    public EffectVisualType GetEffectVisualType() {return fixedElem.display.effectVisualType;}
    public EffectMoveType GetEffectMoveType() {return fixedElem.display.effectMoveType;}
    public Unit GetCaster() {return flexibleElem.display.caster;}
    public int GetRemainPhase() {return flexibleElem.display.remainPhase;}
    public int GetRemainStack() {return flexibleElem.display.remainStack;}
    
    public StatusEffectType GetStatusEffectType() {return fixedElem.actuals[0].statusEffectType;}
    public StatusEffectType GetStatusEffectType(int index) {return fixedElem.actuals[index].statusEffectType;}
    public bool GetIsMultifly() {return  fixedElem.actuals[0].isMultifly;}
    public bool GetIsMultifly(int index) {return  fixedElem.actuals[index].isMultifly;}
    public float GetAmount() {return flexibleElem.actuals[0].amount;}
    public float GetAmount(int index) {return flexibleElem.actuals[index].amount;}
    public float GetRemainAmount() {return flexibleElem.actuals[0].remainAmount;}
    public float GetRemainAmount(int index) {return flexibleElem.actuals[index].remainAmount;}

    public void SetAmount(float amount)
    {
        flexibleElem.actuals[0].amount = amount;
    }

    public void SetAmount(int index, float amount)
    {
        flexibleElem.actuals[index].amount = amount;
    }

    public void SetRemainAmount(float amount)
    {
		flexibleElem.actuals[0].remainAmount = amount;
    }

    public void SetRemainAmount(int index, float amount)
    {
		flexibleElem.actuals[index].remainAmount = amount;
    }

    public void SubAmount(int index, float amount)
    {
        flexibleElem.actuals[index].remainAmount -= amount;
    }

    public void AddRemainPhase(int phase)
	{
		flexibleElem.display.remainPhase += phase;
	}
	
	public void DecreaseRemainPhase()
	{
		flexibleElem.display.remainPhase -= 1;
	}

    public void DecreaseRemainPhase(int phase)
	{
		flexibleElem.display.remainPhase -= phase;
	}
    
    public void SetRemainPhase(int phase)
    {
		flexibleElem.display.remainPhase = phase;
    }
    
    public void AddRemainStack(int stack)
    {
		flexibleElem.display.remainStack += stack;
    }
    
    public void DecreaseRemainStack()
    {
		flexibleElem.display.remainStack -= 1;
    }
    
    public void DecreaseRemainStack(int stack)
    {
		flexibleElem.display.remainStack -= stack;
    }

    public void SetRemainStack(int stack)
    {
		flexibleElem.display.remainStack = stack;
    }

    public bool IsOfType(StatusEffectType statusEffectType)
    {
        bool isOfType = false;
        if (statusEffectType.Equals(this.GetStatusEffectType()))
        {
            isOfType = true;
        }
        
        return isOfType;
    }

    public bool IsOfType(int index, StatusEffectType statusEffectType)
    {
        return statusEffectType.Equals(this.GetStatusEffectType(index));
    }
    
    public bool IsSameStatusEffect(StatusEffect anotherStatusEffect)
    {
        return (this.GetOriginSkillName().Equals(anotherStatusEffect.GetOriginSkillName()) &&
                (this.GetCaster().Equals(anotherStatusEffect.GetCaster())));
    }

    public static float CalculateAmount(int stack, FixedElement.ActualElement fixedElem, Unit caster)
    {
        float seVar;
        if (fixedElem.seVar == StatusEffectVar.Stack)
            seVar = stack;
        else
            seVar = GetSEVar(fixedElem.seVar, caster);
        float seCoef = fixedElem.seCoef;
        float seBase = fixedElem.seBase;

        float result = seVar * seCoef + seBase;

        return result;
    }

    public static float GetSEVar(StatusEffectVar seVarEnum, Unit caster)
    {
        float result = 0;

        if (seVarEnum == StatusEffectVar.Absorption)
        {
            StatusEffect uniqueStatusEffect = caster.GetStatusEffectList().Find(se => se.GetDisplayName() == "흡수");		
            int stack = 0;
            if (uniqueStatusEffect != null)
                stack = uniqueStatusEffect.GetRemainStack();
            result = stack;
        }
        else if (seVarEnum == StatusEffectVar.Absorption_1r)
        {
            StatusEffect uniqueStatusEffect = caster.GetStatusEffectList().Find(se => se.GetDisplayName() == "흡수");		
            int stack = 0;
            if (uniqueStatusEffect != null)
                stack = uniqueStatusEffect.GetRemainStack();

            float power = caster.GetActualStat(Stat.Power);
            Debug.Log("Power : " + power);
            result = (0.6f + (float)stack * 0.1f) * power; Debug.Log("Result : " + result);
        }
        else if (seVarEnum == StatusEffectVar.BuffFromOther)
        {
            result = caster.GetStatusEffectList().Count(
                    x => x.GetIsBuff() && (x.GetCaster() != caster));
        }
        else if (seVarEnum == StatusEffectVar.CurrentHp)
            result = (float)caster.GetCurrentHealth();
        else if (seVarEnum == StatusEffectVar.DamagedAlly)
        {
            UnitManager unitManager = MonoBehaviour.FindObjectOfType<UnitManager>();
            result = unitManager.GetAllUnits().Count(x => (x.GetSide() == Enums.Side.Ally) && 
                                                        ((float)x.GetCurrentHealth()/(float)x.GetMaxHealth() <= 40));
        }
        else if (seVarEnum == StatusEffectVar.Level)
            result = MonoBehaviour.FindObjectOfType<BattleManager>().GetPartyLevel();
        else if (seVarEnum == StatusEffectVar.LostHpPercent)
            result = 100f - (100 * ((float)caster.GetCurrentHealth()/(float)caster.GetMaxHealth()));
        else if (seVarEnum == StatusEffectVar.MetalTile)
        {
            TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();
            List<Tile> nearbyTilesFromLenian = new List<Tile>();
            nearbyTilesFromLenian = tileManager.GetTilesInRange(Enums.RangeForm.Square, caster.GetPosition(), 0, 1, caster.GetDirection());
            result = nearbyTilesFromLenian.Count(x => x.GetTileElement() == Enums.Element.Metal);
        }
        else if (seVarEnum == StatusEffectVar.NearbyEnemy)
        {
            UnitManager unitManager = MonoBehaviour.FindObjectOfType<UnitManager>();
            TileManager tileManager = MonoBehaviour.FindObjectOfType<TileManager>();
            Vector2 unitPosition = caster.GetPosition();
            List<Tile> nearbyTiles = tileManager.GetTilesInRange(RangeForm.Diamond, unitPosition, 1, 3, Direction.LeftUp);

            List<Unit> nearbyUnits = new List<Unit>();
            foreach (var tile in nearbyTiles)
            {
                if (tile.IsUnitOnTile())
                    nearbyUnits.Add(tile.GetUnitOnTile());
            }

            result = nearbyUnits.Count(x => x.GetSide() == Side.Enemy);
        }
        else if (seVarEnum == StatusEffectVar.NearestUnit)
        {
            UnitManager unitManager = MonoBehaviour.FindObjectOfType<UnitManager>();
            List<Unit> exceptItself = unitManager.GetAllUnits().FindAll(x => x.GetNameInCode() == "curi");
            
            result = exceptItself.Min(x => Utility.GetDistance(caster.GetPosition(), x.GetPosition()));
            if (result > 25) result = 25;
        }
        else if (seVarEnum == StatusEffectVar.Power)
            result = caster.GetActualStat(Stat.Power);
        else if (seVarEnum == StatusEffectVar.RemainEnemy)
        {
            UnitManager unitManager = MonoBehaviour.FindObjectOfType<UnitManager>();
            result = unitManager.GetAllUnits().Count(x => x.GetSide() == Enums.Side.Enemy);
        }
        else if (seVarEnum == StatusEffectVar.None)
            result = 0;     
        else if (seVarEnum == StatusEffectVar.Once)
            result = 1;   
        else
            result = 0;

        return result;
    }
}
