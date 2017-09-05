using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Enums;
using GameData;

// This component is used in two UI : SelectedUnitViewer and UnitViewer.
namespace BattleUI{
    public class UnitViewer : UnitInfoUI{
        TileManager tileManager;
        Image unitImage;
        
        GameObject elementBuffIcon;

        // FIXME : 버프/디버프 표시 임시로 텍스트로.
        public GameObject statusEffectIconPrefab;
        Vector3 statusEffectIconBarPosition;
        List<StatusEffectIcon> statusEffectIcons;
        Text statusEffectText;
        public Image HpBar;
        public Image CurrentApBar;
        public Image NextApBar;
        public Image AfterApBar;
        public GameObject ApBarArrow;

        //UpdateUnitViewer가 2개 있는데, 위의 것은 Battle / 아래 것은 BattleReady 씬에서 사용
        public void UpdateUnitViewer(Unit unit) {
            base.unit = unit;
            unitImage.sprite = unit.GetDefaultSprite();
            CheckElementBuff(unit);
            UpdateHpBar(unit);
            UpdateApBar(unit);
            UpdateEffect(unit);
            SetCommonUnitInfoUI();
        }

        public void UpdateUnitViewer(string unitName) {
            Debug.Assert(unitName != "unselected");
            string hpString = UnitInfo.GetStat(unitName, Stat.MaxHealth).ToString();
            hpText.text = hpString + "/" + hpString;
            powerText.text = UnitInfo.GetStat(unitName, Stat.Power).ToString();
            defenseText.text = UnitInfo.GetStat(unitName, Stat.Defense).ToString();
            resistText.text = UnitInfo.GetStat(unitName, Stat.Resistance).ToString();

            int Agility = UnitInfo.GetStat(unitName, Stat.Agility);
            int level = PartyData.level;
            apText.text = level + 60 + (Agility / 2) + "(+" + Agility + ")";

            Utility.SetClassImage(classImage, UnitInfo.GetUnitClass(unitName));
            Utility.SetElementImage(elementImage, UnitInfo.GetElement(unitName));
            Utility.SetCelestialImage(celestialImage, UnitInfo.GetCelestial(unitName));
        }

        public void RefreshStatusEffectIconList() {
            foreach(var statusEffectIcon in statusEffectIcons) {
                Destroy(statusEffectIcon.gameObject);
            }
            statusEffectIcons = new List<StatusEffectIcon>();
        }

        public void Clear() {
            Sprite transparentSprite = Resources.Load<Sprite>("Icon/Empty");

            hpText.text = "--/--";
            powerText.text = "";
            defenseText.text = "";
            resistText.text = "";
            apText.text = "--(+--)";
            unitName.text = "--";
            classImage.sprite = transparentSprite;
            elementImage.sprite = transparentSprite;
            celestialImage.sprite = transparentSprite;
            unitImage.sprite = transparentSprite;
        }

        void UpdateEffect(Unit unit){
            RefreshStatusEffectIconList();
            List<UnitStatusEffect> effectList = unit.GetStatusEffectList();
            int numberOfEffects = effectList.Count;
            for (int i = 0; i < numberOfEffects; i++){
                StatusEffectIcon statusEffectIcon = Instantiate(statusEffectIconPrefab).GetComponent<StatusEffectIcon>();
                statusEffectIcon.statusEffect = effectList[i];
                statusEffectIcon.transform.SetParent(transform);
                statusEffectIcon.UpdateSprite();
                statusEffectIcon.UpdatePosition(statusEffectIconBarPosition, i);
                statusEffectIcons.Add(statusEffectIcon);
            }
        }

        void CheckElementBuff(Unit unit) {
            elementBuffIcon.SetActive(false);

            if (unit.GetElement() == tileManager.GetTile(unit.GetPosition()).GetTileElement() && unit.GetElement() != Element.None){
                elementBuffIcon.SetActive(true);
            }
        }

        public void UpdateHpBar(Unit unit){
            HpBar.color = HealthViewer.SideToHealthColor(unit.GetSide());
            HpBar.fillAmount = unit.GetHpRatio();
        }

        void UpdateApBar(Unit unit) {
            CurrentApBar.fillAmount = unit.GetApRatio(unit.GetCurrentActivityPoint());
            AfterApBar.fillAmount = unit.GetApRatio(unit.GetCurrentActivityPoint());
            NextApBar.fillAmount = unit.GetApRatio(unit.GetCurrentActivityPoint()+unit.GetStat(Stat.Agility));
            ApBarArrow.GetComponent<RectTransform>().localPosition = new Vector3(unit.GetApRatio(PartyData.level+60)*175, -14, 0);
        }

        public void PreviewAp(Unit unit, int costAP){
            AfterApBar.fillAmount = unit.GetApRatio(unit.GetCurrentActivityPoint() - costAP);
        }

        public void OffPreviewAp(){
            AfterApBar.fillAmount = CurrentApBar.fillAmount;
        }
        void Awake() {
            tileManager = FindObjectOfType<TileManager>();

            unitImage = transform.Find("UnitImage").GetComponent<Image>();
            unitName = transform.Find("NameText").GetComponent<Text>();
            classImage = transform.Find("ClassImageMask").Find("ClassImage").GetComponent<Image>();

            elementImage = transform.Find("ElementImageMask").Find("ElementImage").GetComponent<Image>();
            elementBuffIcon = transform.Find("ElementBuffImage").gameObject;

            celestialImage = transform.Find("CelestialImageMask").Find("CelestialImage").GetComponent<Image>();

            hpText = transform.Find("HP").Find("HPText").GetComponent<Text>();
            apText = transform.Find("AP").Find("APText").GetComponent<Text>();

            powerText = transform.Find("Power").Find("PowerText").GetComponent<Text>();
            defenseText = transform.Find("Defense").Find("DefenseText").GetComponent<Text>();
            resistText = transform.Find("Resistance").Find("ResistanceText").GetComponent<Text>();

            //효과 표시 내용은 BattleReady씬에서 켜면 에러가 생기기 때문에 씬 이름으로 조건 확인하고 실행
            if(SceneManager.GetActiveScene().name == "Battle"){
                statusEffectIconBarPosition = transform.Find("StatusEffectIcons").GetComponent<RectTransform>().localPosition;
                statusEffectIcons = new List<StatusEffectIcon>();
				//원래 Speed도 위에 들어가야 하는데 준비창이 준비되지 않아서 임시로 if문 안ㅔ 넣어둠
				speedText = transform.Find("Speed").Find("SpeedText").GetComponent<Text> ();
            }
        }

        void Start() {
            elementBuffIcon.SetActive(false);
        }
    }
}