using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using Enums;
using GameData;

public class BattleTriggerManager : MonoBehaviour {
    private static BattleTriggerManager instance;
    public static BattleTriggerManager Instance {
        get { return instance; }
    }
    UnitManager unitManager;
	public SceneLoader sceneLoader;
	ResultPanel resultPanel;

	public List<BattleTrigger> triggers = new List<BattleTrigger>();
	public List<Vector2> targetTiles = new List<Vector2>();
	List<string> reachedTargetUnitNames = new List<string>();

	public string nextScriptName;

	public SceneLoader SceneLoader { get { return sceneLoader; } }
	public List<Vector2> TargetTiles { get { return targetTiles; } }
	public List<string> ReachedTargetUnitNames { get { return reachedTargetUnitNames; } }

	public List<BattleTrigger> ActiveTriggers{
		get{
			return triggers.FindAll(trig => isTriggerActive(trig));
		}
	}

	public void CountBattleTrigger(BattleTrigger trigger) {
        if (trigger.resultType == TrigResultType.Trigger) {
            trigger.Trigger();
        }

		if(trigger.actionType == TrigActionType.UnderCount){
			if(CountUnitOfCondition(trigger) <= trigger.reqCount){
				ActivateTrigger(trigger);
			}
		}
		else{
			trigger.count += 1;
			Debug.Log("Trigger counting : " + trigger.korName + ", " + trigger.count + " / " + trigger.reqCount);
			if (trigger.count == trigger.reqCount) {
				ActivateTrigger(trigger);
			} else if (trigger.repeatable && trigger.acquired){
				BattleData.rewardPoint += trigger.reward;
			}
		}
	}

	void ActivateTrigger(BattleTrigger trigger){
		if(trigger.acquired == trigger.reverse){
			trigger.acquired = !trigger.reverse;
			Debug.Log ("Trigger Activated : " + trigger.korName + " / " + trigger.acquired);
            if (trigger.resultType == TrigResultType.Lose) {
                Debug.Log("Mission FAIL : " + trigger.korName);
                LoadLoseScene();
            }
		}else{
			Debug.Log("This trigger is already Applied.");
		}
	}

	public void WinGame(){
		CheckExtraTriggersAtWinGame();
		StartResultPanel();
	}

	//게임 종료시에 한꺼번에 체크해야 하는 트리거.
	void CheckExtraTriggersAtWinGame(){
		List<BattleTrigger> exTrigs = triggers.FindAll(trig => trig.extra);
		if(exTrigs.Count != 0){
			if(SceneData.stageNumber == 1){
				List<HPChangeLog> TargetLogs = LogsOfType<HPChangeLog>().FindAll(log =>
					(log.target.GetNameEng() == "reina" || log.target.GetNameEng() == "lucius") && log.amount < 0);
				if(!TargetLogs.Any(log => log.target.GetNameEng() == "lucius") || !TargetLogs.Any(log => log.target.GetNameEng() == "reina")){
					exTrigs[0].acquired = true; //한 명이라도 피해를 입지 않았으면 발동
				}
			}else if(SceneData.stageNumber == 2){
				List<ActiveSkill> usedSkills = new List<ActiveSkill>();
				LogsOfType<CastLog>().ForEach(log => {
					if(!usedSkills.Any(skill => skill == log.casting.Skill)){
						usedSkills.Add(log.casting.Skill);
					}
				});
				if(usedSkills.Count >= 7){
					exTrigs[0].acquired = true; //7가지 이상 기술을 사용했으면 발동
				}
			}
		}
	}

	List<T> LogsOfType<T>(){
		List<T> result = new List<T>();
		BattleData.logDisplayList.ForEach(dsp => {
			if(dsp.log is T){
				result.Add((T)Convert.ChangeType(dsp.log, typeof(T)));
			}
		});
		return result;
	}

	public void StartResultPanel(){
		if(!resultPanel.gameObject.activeSelf){
			resultPanel.gameObject.SetActive(true);
			resultPanel.Initialize();
		}
	}
	public void LoadLoseScene(){
		sceneLoader.LoadNextDialogueScene ("Scene_Lose" + SceneData.stageNumber);
	}

	void Awake(){
        instance = this;
		if (!SceneData.isTestMode && !SceneData.isStageMode){
            GameDataManager.Load();
		}
		resultPanel = FindObjectOfType<ResultPanel>();
		resultPanel.Checker = this;
		resultPanel.gameObject.SetActive(false);

		triggers = Parser.GetParsedData<BattleTrigger>();
		nextScriptName = triggers.Find(x => x.resultType == TrigResultType.Info).nextSceneIndex;

        if (FindObjectOfType<ConditionPanel>() != null) {
            FindObjectOfType<CameraMover>().SetMovable(false);
            FindObjectOfType<ConditionPanel>().Initialize(triggers);
        }
	}
	void Start () {
		unitManager = BattleData.unitManager;
		sceneLoader = FindObjectOfType<SceneLoader>();
	}

	public void CountTriggers(TrigActionType actionType, Unit unit = null, string subType = "", Log log = null, Tile dest = null){
		List<BattleTrigger> availableTriggers = ActiveTriggers.FindAll(trig => 
			CheckUnitType(trig, unit) && CheckActionType(trig, actionType) && !trig.logs.Any(item => item == log)
		); //UnitType, ActionType이 일치하고 이미 같은 log로 기록되지 않은 경우

		List<BattleTrigger> targetTriggers;
		if(dest != null){
			targetTriggers = availableTriggers.FindAll(trig => dest.IsReachPosition);
		}else{
			targetTriggers = availableTriggers.FindAll(trig => CheckSubType(trig, subType));
		} //ReachPosition의 경우를 예외 처리.
		
		targetTriggers.ForEach(trig => {
			trig.units.Add(unit);
			if(log != null){
				trig.logs.Add(log);
			}
			CountBattleTrigger(trig);
		});
	}

	//트리거가 현재 활성화되어있는지 여부를 확인.
	//relation == Sequence이고 앞번째 트리거가 달성되지 않은 경우만 false, 그 외에 전부 true.
	public bool isTriggerActive(BattleTrigger trigger){
		if(trigger.resultType == TrigResultType.Info){
			return false;
		}else if(trigger.resultType == TrigResultType.Bonus){
			return true;
		}else{
			List<BattleTrigger> checkList;
			BattleTrigger.TriggerRelation relation;
			if(trigger.resultType == TrigResultType.Win){
				checkList = triggers.FindAll(trig => trig.resultType == TrigResultType.Win);
				relation = triggers.Find(trig => trig.resultType == TrigResultType.Info).winTriggerRelation;
			}else{
				checkList = triggers.FindAll(trig => trig.resultType == TrigResultType.Lose);
				relation = triggers.Find(trig => trig.resultType == TrigResultType.Info).loseTriggerRelation;
			}

			if(relation == BattleTrigger.TriggerRelation.Sequence){
				for(int i = 0; i < checkList.Count; i++){
					if(checkList[i] == trigger){
						if(i == 0){
							return true;
						}else{
							return checkList[i-1].acquired;
						}
					}
				}
				Debug.LogError("checkList에 trigger가 없습니다!");
				return false;
			}else{
				return true;
			}
		}
	}

	int CountUnitOfCondition(BattleTrigger trigger){
		int result = 0;
		foreach(var unit in unitManager.units){
			if(CheckUnitType(trigger, unit)){
				result += 1;
			}
		}
		return result;
	}

	public bool CheckUnitType(BattleTrigger trigger, Unit unit){
		if(unit == null){ //TrigActionType.Phase 등 명시적인 행위주체가 없는 경우
			return true;
		}else if (trigger.unitType == TrigUnitType.Target && trigger.targetUnitNames.Any (x => x.Equals(unit.GetNameEng())))
			return true;
		else if(trigger.unitType == TrigUnitType.Ally && unit.GetSide() == Side.Ally)
			return true;
		else if(trigger.unitType == TrigUnitType.Enemy && unit.GetSide() == Side.Enemy)
			return true;
		else if(trigger.unitType == TrigUnitType.PC && unit.IsPC == true && unit.GetSide() == Side.Ally)
			return true;
		else if(trigger.unitType == TrigUnitType.NeutralChar && unit.IsObject == false && unit.GetSide() == Side.Neutral)
			return true;
		else
			return false;
	}

	bool CheckActionType(BattleTrigger trigger, TrigActionType actionType){
		return trigger.actionType == actionType;
	}

	bool CheckSubType(BattleTrigger trigger, string subType){
		if(trigger.actionType == TrigActionType.MultiAttack){
			return int.Parse(subType) >= int.Parse(trigger.subType);
		}else{
			return trigger.subType == subType;
		}
	}
}