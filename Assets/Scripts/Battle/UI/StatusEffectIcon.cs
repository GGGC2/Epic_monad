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

        public void UpdateSprite() {
            image.sprite = Resources.Load<Sprite>("Icon/Stat/attack");
            image.rectTransform.sizeDelta = new Vector2(WIDTH, HEIGHT);
        }

        public void UpdatePosition(Vector3 pivot, int index) {
            transform.localPosition = pivot + new Vector3(index * (WIDTH + MARGIN), 0, 0);
        }

        public void TriggerPointerEnter() {
            FindObjectOfType<UIManager>().ActivateStatusEffectDisplayPanelAndSetText(transform.position, statusEffect);
        }

        public void TriggerPointerExit() {
            FindObjectOfType<UIManager>().DisableStatusEffectDisplayPanel();
        }
    }
}
