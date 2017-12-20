using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Enums;
using System.Collections.Generic;
using GameData;

namespace BattleUI {
    class StatusEffectIcon : MonoBehaviour {
        public StatusEffect statusEffect;
        public Image image;
        public static readonly float WIDTH = 20;
        public static readonly float HEIGHT = 20;
        static readonly float MARGIN = 5;

        string GetSkillColumnRow(Skill skill) {
            if(skill.requireLevel == 0)
                return "Passive";
            string column = "";
            switch(skill.row) {
            case 1:
                column = "Left";
            break;
            case 2:
                column = "Mid";
            break;
            case 3:
                column = "Right";
            break;
            }
            string row = ((skill.requireLevel + 6)/7).ToString();
            return column + row;
        }

        public void UpdateSprite() {
            string fileName = "";
            if (statusEffect.GetOwnerOfSkill() == "tile" && statusEffect is UnitStatusEffect)
                Utility.SetElementImage(image, ((UnitStatusEffect)statusEffect).GetOwner().GetElement());
            else {
                if (statusEffect.GetOriginSkill() != null) fileName = GetSkillColumnRow(statusEffect.GetOriginSkill());
                string fileDirectory = "Icon/Skill/" + statusEffect.GetCaster().EngName + "/" + fileName;
                try {
                    image.sprite = Resources.Load<Sprite>(fileDirectory);
                }
                catch (UnityException e) {
                    image.sprite = Resources.Load<Sprite>("Icon/Empty");
                }
            }
            image.rectTransform.sizeDelta = new Vector2(WIDTH, HEIGHT);
        }

        public void UpdatePosition(Vector3 pivot, int index) {
            transform.localPosition = pivot + new Vector3(index * (WIDTH + MARGIN) * FindObjectOfType<RectTransform>().localScale.x, 0, 0);
        }

        public void TriggerPointerEnter() {
            FindObjectOfType<UIManager>().ActivateStatusEffectDisplayPanelAndSetText(transform.position, statusEffect);
        }

        public void TriggerPointerExit() {
            FindObjectOfType<UIManager>().DisableStatusEffectDisplayPanel();
        }
    }
}
