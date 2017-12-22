using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enums;

public class BattleTrigger{
	public bool acquired;
	public TrigResultType result;
	public TrigActionType action;
	public TrigUnitType target;
	public TrigUnitType actor;

	public string subType = "";
	public int reward;
	public int count;
	public int reqCount;
	
	public bool reverse; //일반적인 경우와 반대로, 달성된 상태로 시작해서 조건부로 해제되는 것들. 예) n페이즈 이내 승리
	public bool repeatable;

	public List<string> targetUnitNames = new List<string>();
	public List<string> actorUnitNames = new List<string>();
	public List<Vector2> targetTiles = new List<Vector2>();
	public string nextSceneIndex;
	public string korName;
	
	//승리&패배 조건이 전부 만족시켜야 하는지/하나만 만족시켜도 되는지에 대한 정보
	public enum TriggerRelation{One, All, Sequence}
	public TriggerRelation winTriggerRelation;
	public TriggerRelation loseTriggerRelation;

    public List<Unit> units = new List<Unit>(); // 이 trigger를 count시킨 유닛들
	public List<Log> logs = new List<Log>(); // 이 trigger를 count시킨 로그들

	public BattleTrigger(string data, TrigResultType resultType, StringParser commaParser){
        // BattleTriggerFactory에서 commaParser를 이용해 ResultType은 파싱해놓은 상태
		if(resultType == TrigResultType.Info) {
			nextSceneIndex = commaParser.Consume();
			winTriggerRelation = commaParser.ConsumeEnum<TriggerRelation>();
			loseTriggerRelation = commaParser.ConsumeEnum<TriggerRelation>();
		}else{
			reqCount = commaParser.ConsumeInt();
			reward = commaParser.ConsumeInt();
			korName = commaParser.Consume();
			action = commaParser.ConsumeEnum<TrigActionType>();
			//여기까지는 Info를 제외한 모든 트리거에 필요한 정보

			//action의 번호의 의미에 대해서는 TrigActionType을 정의한 부분에서 각주 참고.
			if(action == TrigActionType.Escape){
				targetTiles = new List<Vector2>();
				int numberOfTiles = commaParser.ConsumeInt();
				for (int i = 0; i < numberOfTiles; i++){
					int x = commaParser.ConsumeInt();
					int y = commaParser.ConsumeInt();
					Vector2 position = new Vector2(x, y);
					targetTiles.Add(position);
				}
			}else if((int)action >= 11 && (int)action <= 13){
				subType = commaParser.Consume();
			}

			if((int)action > 10){
				actor = commaParser.ConsumeEnum<TrigUnitType>();
				if(actor == TrigUnitType.Name){
					actorUnitNames = GetNameList(commaParser);
				}

				if((int)action > 20){
					target = commaParser.ConsumeEnum<TrigUnitType>();
					if(target == TrigUnitType.Name){
						targetUnitNames = GetNameList(commaParser);
					}
				}	
			}

			//Debug.Log("index : " + commaParser.index + " / length : " + commaParser.origin.Length);
			while(commaParser.index < commaParser.origin.Length){
				string code = commaParser.Consume();
				if(code == ""){
					break;
				}else if(code == "Reverse"){
					reverse = true;
					acquired = true;
				}else if(code == "Repeat"){
					repeatable = true;
				}else{
					Debug.LogError("잘못된 트리거 속성 이름 : index " + commaParser.index + " / " + code);
				}
			}
		}
	}

	List<string> GetNameList(StringParser commaParser){
		int targetCount = commaParser.ConsumeInt();
		List<string> result = new List<string>();
		for (int i = 0; i < targetCount; i++){
			string unitName = commaParser.Consume();
			result.Add(unitName);
		}

		return result;
	}

    public virtual void Trigger() {

    }
}

class BattleTriggerFactory {
    public static BattleTrigger Get(string data) {
        StringParser commaParser = new StringParser(data, '\t');
        TrigResultType resultType = commaParser.ConsumeEnum<TrigResultType>();
        BattleTrigger trigger = null;
        if(resultType != TrigResultType.Trigger) {
			trigger = new BattleTrigger(data, resultType, commaParser);
        }
        else {
            string triggerName = commaParser.Consume();
            switch(triggerName) {
            case "14-0":
                trigger = new Stage_14_0_BattleTrigger(data, resultType, commaParser);
                break;
            case "14-1":
                trigger = new Stage_14_1_BattleTrigger(data, resultType, commaParser);
                break;
            }
        }
        if(trigger != null)
            trigger.result = resultType;
        return trigger;
    }
}