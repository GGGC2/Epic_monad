using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using BattleUI.APBarPanels;
using Enums;
using Util;
using GameData;

namespace BattleUI{
	public class APBarPanel : MonoBehaviour{
		private GameObject bigProfile;
		private List<GameObject> otherProfiles = new List<GameObject>();
		private List<GameObject> turnSeperateBars = new List<GameObject>();
		private List<GameObject> currentTurnTexts = new List<GameObject>();
		private List<GameObject> nextTurnTexts = new List<GameObject>();
		private Sprite notFoundProfileSprite;
		private Sprite defaultProfileSprite;
		private Sprite defaultBigProfileSprite;

		private int seperationSpace = 10;

		const int maxUICount = 13;

		void Awake(){
			bigProfile = transform.Find("Big/Profile").gameObject;

			otherProfiles = GetChildObjects(transform.Find("Profiles"));
			turnSeperateBars = GetChildObjects(transform.Find("Bars"));
			currentTurnTexts = GetChildObjects(transform.Find("Currents"));
			nextTurnTexts = GetChildObjects(transform.Find("Nexts"));

			notFoundProfileSprite = Resources.Load("UnitImage/portrait_notfound", typeof(Sprite)) as Sprite;
			Debug.Assert(notFoundProfileSprite != null, "Cannot find 'not found' sprite");
			defaultProfileSprite = Resources.Load("UnitImage/portrait_placeholder", typeof(Sprite)) as Sprite;
			Debug.Assert(defaultProfileSprite != null, "Cannot find 'default' sprite");
			defaultBigProfileSprite = Resources.Load("UnitImage/portrait_placeholder_circle", typeof(Sprite)) as Sprite;
			Debug.Assert(defaultBigProfileSprite != null, "Cannot find 'default big'sprite");
		}

		public void UpdateAPDisplay(List<Unit> allUnits){
			// 오브젝트는 AP바에 표시하지 않는다
			List<Unit> allUnitsExceptObject = allUnits.FindAll(unit => unit.IsObject == false);
			allUnits = allUnitsExceptObject;

			Debug.Assert(allUnits.Count > 0);
			UnitManager unitManager = BattleData.unitManager;
			int standardActivityPoint = unitManager.GetStandardActivityPoint();

			Clear();

			Unit selectedUnit = BattleData.selectedUnit;

			if (selectedUnit == null){
				Debug.LogWarning("There is no selected unit");
				return;
			}

			SetBigProfile(selectedUnit);

			UnitWrapperFactory wrapperFactory = new UnitWrapperFactory(selectedUnit);
			List<UnitWrapper> otherUnits = wrapperFactory.WrapUnits(allUnits);

			if (selectedUnit == otherUnits[0].GetUnit() && BattleData.previewAPAction == null)
			{
				otherUnits.RemoveAt(0);
			}

			otherUnits = SortUnits(otherUnits);
			if (otherUnits.Count > maxUICount - 1)
			{
				otherUnits = otherUnits.GetRange(0, maxUICount - 1);
			}

			SetProfiles(standardActivityPoint, otherUnits);
			SetCurrentText(standardActivityPoint, otherUnits);
			SetNextText(standardActivityPoint, otherUnits);
			SetSeperateBar(standardActivityPoint, otherUnits);
		}

		private void SetBigProfile(Unit unit){
			bigProfile.GetComponent<Image>().enabled = true;
            
			bigProfile.GetComponent<Image>().sprite = FindUnitProfileImage(unit);

			GameObject apTextGO = bigProfile.transform.Find("apText").gameObject;
			apTextGO.GetComponent<CustomUIText>().enabled = true;
			apTextGO.GetComponent<CustomUIText>().text = unit.GetCurrentActivityPoint().ToString();
		}

		void SetProfiles(int standardActivityPoint, List<UnitWrapper> otherUnits){
			if (otherUnits.Count == 0) {return;}

			for (int index = 0; index < otherUnits.Count; index += 1){
				GameObject profileGameObject = otherProfiles[index];

				profileGameObject.GetComponent<Image>().enabled = true;
				SetProfile(profileGameObject, otherUnits[index], standardActivityPoint);
			}

			// Does not make space no units left in current turn.
			if (otherUnits[0].GetUnit().GetCurrentActivityPoint() < standardActivityPoint) {return;}

			for (int index = 0; index < otherUnits.Count; index += 1){
				GameObject profileGameObject = otherProfiles[index];
				UnitWrapper unit = otherUnits[index];

				if (GetActivityPoint(unit) < standardActivityPoint){
					profileGameObject.GetComponent<RectTransform>().anchoredPosition += new Vector2(seperationSpace * 2, 0);
				}
			}
		}

		private void SetCurrentText(int standardActivityPoint, List<UnitWrapper> otherUnits)
		{
			if (otherUnits.Count == 0)
			{
				return;
			}

			UnitWrapper firstUnit = otherUnits[0];
			if (GetActivityPoint(firstUnit) >= standardActivityPoint)
			{
				currentTurnTexts[0].GetComponent<Image>().enabled = true;
			}
			else
			{
				return;
			}
		}

		private void SetNextText(int standardActivityPoint, List<UnitWrapper> otherUnits)
		{
			for (int i=0; i < otherUnits.Count; i+=1)
			{
				UnitWrapper unit = otherUnits[i];
				if (GetActivityPoint(unit) >= standardActivityPoint)
				{
					continue;
				}

				if (nextTurnTexts.Count <= i)
				{
					Debug.Log("Unit is to many for this APBar");
					return;
				}

				GameObject nextTurnText = nextTurnTexts[i];
				nextTurnText.GetComponent<Image>().enabled = true;

				// Does not make space no units left in current turn.
				if (i != 0)
				{
					nextTurnText.GetComponent<RectTransform>().anchoredPosition += new Vector2(seperationSpace * 2, 0);
				}

				return;
			}
		}

		private void SetSeperateBar(int standardActivityPoint, List<UnitWrapper> otherUnits){
			int nextTurnUnitIndex = -1;
			for (int i=0; i < otherUnits.Count; i+=1){
				UnitWrapper unit = otherUnits[i];
				if (GetActivityPoint(unit) < standardActivityPoint){
					nextTurnUnitIndex = i;
					break;
				}
			}

			if (nextTurnUnitIndex == -1)
			{
				return;
			}

			if (nextTurnUnitIndex == 0)
			{
				return;
			}

			int barIndex = nextTurnUnitIndex - 1;
			if (turnSeperateBars.Count <= barIndex)
			{
				Debug.LogWarning("Unit is to many for this APBar");
				return;
			}

			GameObject turnSeperateBar = turnSeperateBars[barIndex];
			turnSeperateBar.GetComponent<Image>().enabled = true;
			turnSeperateBar.GetComponent<RectTransform>().anchoredPosition += new Vector2(seperationSpace, 0);
		}

		private void SetProfile(GameObject profileGO, UnitWrapper unitWrapper, int standardActivityPoint){
			Unit unit = unitWrapper.GetUnit();
			profileGO.GetComponent<Image>().sprite = FindUnitProfileImage(unit);
			profileGO.GetComponent<OrderPortraitSlot>().unit = unitWrapper.GetUnit();

			GameObject apTextGO = profileGO.transform.Find("apText").gameObject;
			apTextGO.GetComponent<CustomUIText>().enabled = true;

			int activityPoint = GetActivityPoint(unitWrapper);
			if (activityPoint < standardActivityPoint)
			{
				activityPoint += unit.GetRegenerationAmount();
			}
			apTextGO.GetComponent<CustomUIText>().text = activityPoint.ToString();

			if (unitWrapper.IsPreviewUnit()){
				GameObject arrow = profileGO.transform.Find("arrow").gameObject;
				arrow.GetComponent<Image>().enabled = true;
			}
		}

		public void FindAndHighlightPortrait(Unit unit){
			foreach(GameObject profileGO in otherProfiles){
				if(profileGO.GetComponent<OrderPortraitSlot>().unit == unit){
					profileGO.GetComponent<Image>().color = Color.blue;
				}
			}
		}

		public void ResetAllPortraitColor(){
			foreach(GameObject profileGO in otherProfiles){
				profileGO.GetComponent<Image>().color = Color.white;
			}
		}

		private Sprite FindUnitProfileImage(Unit unit)
		{
			string imagePath = "UnitImage/portrait_" + unit.GetNameInCode().ToString();

			Sprite sprite = Resources.Load(imagePath, typeof(Sprite)) as Sprite;
			if (sprite == null)
			{
				//Debug.LogWarning("Cannot find sprite for " + unit.GetName() + "(" + unit.GetNameInCode() + ")");
				sprite = notFoundProfileSprite;
			}

			return sprite;
		}

		private void Clear()
		{
			// make invisible all things
			bigProfile.GetComponent<Image>().enabled = false;
			foreach (GameObject profile in otherProfiles)
			{
				profile.GetComponent<Image>().enabled = false;
				profile.GetComponent<DefaultPosition>().ResetPosition();
				profile.transform.Find("apText").GetComponent<CustomUIText>().enabled = false;
				profile.transform.Find("arrow").GetComponent<Image>().enabled = false;
			}

			foreach (GameObject bar in turnSeperateBars)
			{
				bar.GetComponent<Image>().enabled = false;
				bar.GetComponent<DefaultPosition>().ResetPosition();
			}

			foreach (GameObject text in currentTurnTexts)
			{
				text.GetComponent<Image>().enabled = false;
				text.GetComponent<DefaultPosition>().ResetPosition();
			}

			foreach (GameObject text in nextTurnTexts)
			{
				text.GetComponent<Image>().enabled = false;
				text.GetComponent<DefaultPosition>().ResetPosition();
			}
		}

		private List<GameObject> GetChildObjects(Transform parent)
		{
			List<GameObject> resultList = new List<GameObject>();
			foreach (Transform child in parent)
			{
				resultList.Add(child.gameObject);
			}
			return resultList;
		}

		private List<UnitWrapper> SortUnits(List<UnitWrapper> units)
		{
			UnitManager unitManager = BattleData.unitManager;
			int standardActivityPoint =  unitManager.GetStandardActivityPoint();

			List<UnitWrapper> thisTurnUnits =
				units.FindAll(wrapper => GetActivityPoint(wrapper) >= standardActivityPoint);
			List<UnitWrapper> nextTurnUnits =
				units.FindAll(wrapper => GetActivityPoint(wrapper) < standardActivityPoint);

			thisTurnUnits.Sort(SortHelper.Chain(new List<Comparison<UnitWrapper>>
			{
				SortHelper.CompareBy<UnitWrapper>(wrapper => GetActivityPoint(wrapper)),
				SortHelper.CompareBy<UnitWrapper>(wrapper => wrapper.GetUnit().GetStat(Stat.Agility)),
				SortHelper.CompareBy<UnitWrapper>(wrapper => wrapper.GetGameObject().GetInstanceID())
			}, reverse:true));

			nextTurnUnits.Sort(SortHelper.Chain(new List<Comparison<UnitWrapper>>
			{
				SortHelper.CompareBy<UnitWrapper>(wrapper => {
					int ap = GetActivityPoint(wrapper);
					int recover = wrapper.GetUnit().GetStat(Stat.Agility);
					return ap + recover;
				}),
				SortHelper.CompareBy<UnitWrapper>(wrapper => wrapper.GetUnit().GetStat(Stat.Agility)),
				SortHelper.CompareBy<UnitWrapper>(wrapper => wrapper.GetGameObject().GetInstanceID())
			}, reverse:true));

			List<UnitWrapper> result = new List<UnitWrapper>();
			result.AddRange(thisTurnUnits);
			result.AddRange(nextTurnUnits);
			return result;
		}

		private int GetActivityPoint(UnitWrapper wrapper){
			int activityPoint = wrapper.GetUnit().GetCurrentActivityPoint();
			if (wrapper.IsPreviewUnit() && BattleData.previewAPAction != null){
				activityPoint -= BattleData.previewAPAction.requiredAP;
			}
			return activityPoint;
		}
	}
}
