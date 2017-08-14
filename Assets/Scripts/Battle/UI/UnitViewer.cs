using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Enums;
using GameData;

// This component is used in two UI : SelectedUnitViewer and UnitViewer.
namespace BattleUI{
    public class UnitViewer : MonoBehaviour{

        TileManager tileManager;

        Image unitImage;
        Text nameText;
        Image classImage;
        Image elementImage;
        GameObject elementBuffIcon;
        Image celestialImage;

        Text hpText;

        Text apText;

        Text powerText;
        Text defenseText;
		Text resistanceText;
		Text speedText;
        // FIXME : 버프/디버프 표시 임시로 텍스트로.
        public GameObject statusEffectIconPrefab;
        Vector3 statusEffectIconBarPosition;
        List<StatusEffectIcon> statusEffectIcons;
        Text statusEffectText;
        public Image HpBar;
        public Image CurrentApBar;
        public Image NextApBar;
        public GameObject ApBarArrow;

        public void UpdateUnitViewer(Unit unit) {
            unitImage.sprite = unit.GetDefaultSprite();
            nameText.text = unit.GetName();
            SetClassImage(unit.GetUnitClass());
            SetElementImage(unit.GetElement());
            CheckElementBuff(unit);
            SetCelestialImage(unit.GetCelestial());
            UpdateHp(unit);
            UpdateAp(unit);
            UpdatePower(unit);
            UpdateDefense(unit);
			UpdateResistance(unit);
			UpdateSpeed (unit);

            UpdateEffect(unit);
        }

        public void RefreshStatusEffectIconList() {
            foreach(var statusEffectIcon in statusEffectIcons) {
                Destroy(statusEffectIcon.gameObject);
            }
            statusEffectIcons = new List<StatusEffectIcon>();
        }

        public void UpdateUnitViewer(string unitName) {
            Debug.Assert(unitName != "unselected");
            string hpString = UnitInfo.GetStat(unitName, Stat.MaxHealth).ToString();
            hpText.text = hpString + "/" + hpString;
            powerText.text = UnitInfo.GetStat(unitName, Stat.Power).ToString();
            defenseText.text = UnitInfo.GetStat(unitName, Stat.Defense).ToString();
            resistanceText.text = UnitInfo.GetStat(unitName, Stat.Resistance).ToString();

            int Agility = UnitInfo.GetStat(unitName, Stat.Agility);
            int level = PartyData.level;
            apText.text = level + 60 + (Agility / 2) + "(+" + Agility + ")";

            SetClassImage(UnitInfo.GetUnitClass(unitName));
            if (SceneData.stageNumber >= Setting.elementOpenStage)
                SetElementImage(UnitInfo.GetElement(unitName));
            if (SceneData.stageNumber >= Setting.celestialOpenStage)
                SetCelestialImage(UnitInfo.GetCelestial(unitName));
        }

        public void Clear() {
            Sprite transparentSprite = Resources.Load<Sprite>("Icon/Empty");

            hpText.text = "--/--";
            powerText.text = "";
            defenseText.text = "";
            resistanceText.text = "";
            apText.text = "--(+--)";
            nameText.text = "--";
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

            if (unit.GetElement() == tileManager.GetTile(unit.GetPosition()).GetTileElement() && unit.GetElement() != Element.None)
                elementBuffIcon.SetActive(true);
        }

        void UpdateHp(Unit unit){
            hpText.text = unit.GetCurrentHealth() + " / " + unit.GetStat(Stat.MaxHealth);
            HpBar.color = HealthViewer.SideToHealthColor(unit.side);
            HpBar.fillAmount = unit.GetHpRatio();
        }

        void UpdateAp(Unit unit) {
            apText.text = unit.GetCurrentActivityPoint() + " (+" + unit.GetStat(Stat.Agility) + ")";
            CurrentApBar.fillAmount = unit.GetApRatio(unit.GetCurrentActivityPoint());
            NextApBar.fillAmount = unit.GetApRatio(unit.GetCurrentActivityPoint()+unit.GetStat(Stat.Agility));
            ApBarArrow.GetComponent<RectTransform>().localPosition = new Vector3(unit.GetApRatio(PartyData.level+60)*175, -14, 0);
        }

        void UpdatePower(Unit unit) {
            int actualPower = unit.GetStat(Stat.Power);
            int originPower = unit.GetBaseStat(Stat.Power);

            powerText.color = Color.white;
            powerText.text = actualPower.ToString();
            if (actualPower > originPower)
                powerText.color = Color.green;
            else if (actualPower < originPower)
                powerText.color = Color.red;
        }

        void UpdateDefense(Unit unit) {
            int actualDefense = unit.GetStat(Stat.Defense);
            int originDefense = unit.GetBaseStat(Stat.Defense);

            defenseText.color = Color.white;
            defenseText.text = actualDefense.ToString();
            if (actualDefense > originDefense)
                defenseText.color = Color.green;
            else if (actualDefense < originDefense)
                defenseText.color = Color.red;
        }

        void UpdateResistance(Unit unit) {
            int actualResistance = unit.GetStat(Stat.Resistance);
            int originResistance = unit.GetBaseStat(Stat.Resistance);

            resistanceText.color = Color.white;
            resistanceText.text = actualResistance.ToString();
            if (actualResistance > originResistance)
                resistanceText.color = Color.green;
            else if (actualResistance < originResistance)
                resistanceText.color = Color.red;
        }

		void UpdateSpeed(Unit unit) {
			int actualSpeed = (int)unit.GetSpeed ();
			int originSpeed = 100;

			speedText.color = Color.white;
			speedText.text = actualSpeed.ToString ();
			if (actualSpeed > originSpeed)
				speedText.color = Color.green;
			else if (actualSpeed < originSpeed)
				speedText.color = Color.red;
		}

        void SetClassImage(UnitClass unitClass) {
            if (unitClass == UnitClass.Melee)
                classImage.sprite = Resources.Load<Sprite>("Icon/Stat/meleeClass");
            else if (unitClass == UnitClass.Magic)
                classImage.sprite = Resources.Load<Sprite>("Icon/Stat/magicClass");
            else
                classImage.sprite = Resources.Load<Sprite>("Icon/Empty");
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

        void SetCelestialImage(Celestial celestial) {
            if (celestial == Celestial.Sun)
                celestialImage.sprite = Resources.Load("Icon/Celestial/sun", typeof(Sprite)) as Sprite;
            else if (celestial == Celestial.Moon)
                celestialImage.sprite = Resources.Load("Icon/Celestial/moon", typeof(Sprite)) as Sprite;
            else if (celestial == Celestial.Earth)
                celestialImage.sprite = Resources.Load("Icon/Celestial/earth", typeof(Sprite)) as Sprite;
            else
                celestialImage.sprite = Resources.Load("Icon/Empty", typeof(Sprite)) as Sprite;
        }

        void Awake() {
            tileManager = FindObjectOfType<TileManager>();

            unitImage = transform.Find("UnitImage").GetComponent<Image>();
            nameText = transform.Find("NameText").GetComponent<Text>();
            classImage = transform.Find("ClassImage").GetComponent<Image>();

            elementImage = transform.Find("ElementImage").GetComponent<Image>();
            elementBuffIcon = transform.Find("ElementImage").Find("BuffImage").gameObject;

            celestialImage = transform.Find("CelestialImage").GetComponent<Image>();

            hpText = transform.Find("HP").Find("HPText").GetComponent<Text>();
            apText = transform.Find("AP").Find("APText").GetComponent<Text>();

            powerText = transform.Find("Power").Find("PowerText").GetComponent<Text>();
            defenseText = transform.Find("Defense").Find("DefenseText").GetComponent<Text>();
            resistanceText = transform.Find("Resistance").Find("ResistanceText").GetComponent<Text>();
			speedText = transform.Find("Speed").Find("SpeedText").GetComponent<Text> ();

            //효과 표시 내용은 BattleReady씬에서 켜면 에러가 생기기 때문에 씬 이름으로 조건 확인하고 실행
            if(SceneManager.GetActiveScene().name == "Battle"){
                statusEffectIconBarPosition = transform.Find("StatusEffectIcons").GetComponent<RectTransform>().localPosition;
                statusEffectIcons = new List<StatusEffectIcon>();
            }
        }

        void Start() {
            elementBuffIcon.SetActive(false);
        }
    }
}