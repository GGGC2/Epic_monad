using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace BattleUI {
    class StatusEffectDisplayPanel : MonoBehaviour {
        Text displayName;
        Text originSkillAndCaster;
        Text explanation;
        Text remainPhase;

        void Awake() {
            displayName = transform.Find("DisplayName").GetComponent<Text>();
            originSkillAndCaster = transform.Find("OriginSkillAndCaster").GetComponent<Text>();
            explanation = transform.Find("Explanation").GetComponent<Text>();
            remainPhase = transform.Find("RemainPhase").GetComponent<Text>();
        }
        public void SetText(StatusEffect statusEffect) {
            displayName.text = statusEffect.GetDisplayName();
            if (statusEffect.GetType() == typeof(UnitStatusEffect)) {
                if (((UnitStatusEffect)statusEffect).GetIsBuff()) displayName.color = Color.green;
                else displayName.color = Color.red;
            }
            
            originSkillAndCaster.text = statusEffect.GetOriginSkillName() + "(" + statusEffect.GetCaster().GetNameKor() + ")";
            explanation.text = statusEffect.GetExplanation();
            remainPhase.text = "";
            if (!statusEffect.GetIsInfinite())  remainPhase.text += "남은 페이즈(" + statusEffect.GetRemainPhase() + ")";
        }
    }
}
