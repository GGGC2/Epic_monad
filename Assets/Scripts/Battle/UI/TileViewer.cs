using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.UI;
using Enums;

namespace BattleUI {
    public class TileViewer : MonoBehaviour {

        Image tileImage;
        Text nameText;
        Text apText;
        Image elementImage;

        public GameObject statusEffectIconPrefab;
        Vector3 statusEffectIconBarPosition;
        List<StatusEffectIcon> statusEffectIcons;
        Text statusEffectText;

        public void RefreshStatusEffectIconList() {
            foreach (var statusEffectIcon in statusEffectIcons) {
                Destroy(statusEffectIcon.gameObject);
            }
            statusEffectIcons = new List<StatusEffectIcon>();
        }

        public void UpdateTileViewer(Tile tile) {
            if (tileImage == null) {

                Debug.LogError("TileImage is not exist : " + gameObject.GetInstanceID());
            }
            tileImage.sprite = tile.gameObject.GetComponent<SpriteRenderer>().sprite;
            nameText.text = tile.GetTileName();
            int requiredAP = tile.GetBaseMoveCost();
            int unclimbableAP = 200;
            if (requiredAP < unclimbableAP) {
                apText.text = "소모 행동력 " + requiredAP;
            } else {
                apText.text = "-";
            }

            SetElementImage(tile.GetTileElement());
            UpdateEffect(tile);
            UIManager.Instance.DisableStatusEffectDisplayPanel();
        }

        void UpdateEffect(Tile tile) {
            RefreshStatusEffectIconList();
            List<TileStatusEffect> effectList = tile.GetStatusEffectList();
            int numberOfEffects = effectList.Count;
            for (int i = 0; i < numberOfEffects; i++) {
                StatusEffectIcon statusEffectIcon = Instantiate(statusEffectIconPrefab).GetComponent<StatusEffectIcon>();
                statusEffectIcon.statusEffect = effectList[i];
                statusEffectIcon.transform.SetParent(transform);
                statusEffectIcon.UpdateSprite();
                statusEffectIcon.UpdatePosition(statusEffectIconBarPosition, i);
                statusEffectIcons.Add(statusEffectIcon);
            }
        }

        void SetElementImage(Element element) {
            if (element == Element.Fire)
                elementImage.sprite = Resources.Load("Icon/Element/fire", typeof(Sprite)) as Sprite;
            else if (element == Element.Water)
                elementImage.sprite = Resources.Load("Icon/Element/water", typeof(Sprite)) as Sprite;
            else if (element == Element.Plant)
                elementImage.sprite = Resources.Load("Icon/Element/plant", typeof(Sprite)) as Sprite;
            else if (element == Element.Metal)
                elementImage.sprite = Resources.Load("Icon/Element/metal", typeof(Sprite)) as Sprite;
            else
                elementImage.sprite = Resources.Load("Icon/Empty", typeof(Sprite)) as Sprite;
        }

        void Awake() {
            tileImage = transform.Find("TileImage").GetComponent<Image>();
            nameText = transform.Find("NameText").GetComponent<Text>();
            apText = transform.Find("APText").GetComponent<Text>();
            elementImage = transform.Find("ElementImageMask").Find("ElementImage").GetComponent<Image>();

            //효과 표시 내용은 BattleReady씬에서 켜면 에러가 생기기 때문에 씬 이름으로 조건 확인하고 실행
            if (SceneManager.GetActiveScene().name == "Battle") {
                statusEffectIconBarPosition = transform.Find("StatusEffectIcons").GetComponent<RectTransform>().localPosition;
                statusEffectIcons = new List<StatusEffectIcon>();
            }
        }
    }
}
