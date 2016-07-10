using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

using BattleUI.APBarPannels;

namespace BattleUI
{
	public class APBarPannel : MonoBehaviour {
		private GameObject bigProfile;
		private List<GameObject> otherProfiles = new List<GameObject>();
		private List<GameObject> turnSeperateBars = new List<GameObject>();
		private List<GameObject> currentTurnTexts = new List<GameObject>();
		private List<GameObject> nextTurnTexts = new List<GameObject>();

		private Sprite notFoundProfileSprite;
		private Sprite defaultProfileSprite;
		private Sprite defaultBigProfileSprite;

		private int seperationSpace = 10;

		private void Awake()
		{
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

		public void UpdateAPDisplay(BattleData battleData, List<GameObject> allUnits)
		{
			Debug.Assert(allUnits.Count > 0);
			UnitManager unitManager = battleData.unitManager;
			int standardActionPoint =  unitManager.GetStandardActionPoint();

			Clear();

			GameObject selectedUnit = battleData.selectedUnitObject;

			if (selectedUnit == null)
			{
				Debug.LogWarning("There is no selected unit");
				return;
			}

			SetBigProfile(selectedUnit);

			bool showPreviewUnit = false;
			if (selectedUnit != allUnits[0])
			{
				showPreviewUnit = true;
			}

			List<UnitWrapper> otherUnits = new List<UnitWrapper>();
			UnitWrapperFactory wrapperFactory = new UnitWrapperFactory(selectedUnit);
			otherUnits = wrapperFactory.WrapUnits(allUnits);

			if (!showPreviewUnit)
			{
				otherUnits.RemoveAll(wrapper => wrapper.GetGameObject() == selectedUnit);
			}

			SetProfiles(standardActionPoint, otherUnits);
			SetCurrentText(standardActionPoint, otherUnits);
			SetNextText(standardActionPoint, otherUnits);
			SetSeperateBar(standardActionPoint, otherUnits);
		}

		private void SetBigProfile(GameObject unitGO)
		{
			bigProfile.GetComponent<Image>().enabled = true;

			Unit unit = unitGO.GetComponent<Unit>();
			bigProfile.GetComponent<Image>().sprite = FindUnitProfileImage(unit);

			GameObject apTextGO = bigProfile.transform.Find("apText").gameObject;
			apTextGO.GetComponent<CustomUIText>().enabled = true;
			apTextGO.GetComponent<CustomUIText>().text = unit.activityPoint.ToString();
		}

		private void SetProfiles(int standardActionPoint, List<UnitWrapper> otherUnits)
		{
			if (otherUnits.Count == 0)
			{
				return;
			}

			for (int index = 0; index < otherUnits.Count; index += 1)
			{
				GameObject profileGameObject = otherProfiles[index];

				profileGameObject.GetComponent<Image>().enabled = true;
				SetProfile(profileGameObject, otherUnits[index], standardActionPoint);
			}

			// Does not make space no units left in current turn.
			if (otherUnits[0].GetUnit().GetCurrentActivityPoint() < standardActionPoint)
			{
				return;
			}

			for (int index = 0; index < otherUnits.Count; index += 1)
			{
				GameObject profileGameObject = otherProfiles[index];
				Unit unit = otherUnits[index].GetUnit();

				if (unit.GetCurrentActivityPoint() < standardActionPoint)
				{
					profileGameObject.GetComponent<RectTransform>().anchoredPosition += new Vector2(seperationSpace * 2, 0);
				}
			}
		}

		private void SetCurrentText(int standardActionPoint, List<UnitWrapper> otherUnits)
		{
			if (otherUnits.Count == 0)
			{
				return;
			}

			Unit firstUnit = otherUnits[0].GetUnit();
			if (firstUnit.GetCurrentActivityPoint() >= standardActionPoint)
			{
				currentTurnTexts[0].GetComponent<Image>().enabled = true;
			}
			else
			{
				return;
			}
		}

		private void SetNextText(int standardActionPoint, List<UnitWrapper> otherUnits)
		{
			for (int i=0; i < otherUnits.Count; i+=1)
			{
				Unit unit = otherUnits[i].GetUnit();
				if (unit.GetCurrentActivityPoint() >= standardActionPoint)
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

		private void SetSeperateBar(int standardActionPoint, List<UnitWrapper> otherUnits)
		{
			int nextTurnUnitIndex = -1;
			for (int i=0; i < otherUnits.Count; i+=1)
			{
				Unit unit = otherUnits[i].GetUnit();
				if (unit.GetCurrentActivityPoint() < standardActionPoint)
				{
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

		private void SetProfile(GameObject profileGO, UnitWrapper unitWrapper, int standardActionPoint)
		{
			Unit unit = unitWrapper.GetUnit();
			profileGO.GetComponent<Image>().sprite = FindUnitProfileImage(unit);

			GameObject apTextGO = profileGO.transform.Find("apText").gameObject;
			apTextGO.GetComponent<CustomUIText>().enabled = true;
			int activityPoint = unit.activityPoint;
			if (activityPoint < standardActionPoint)
			{
				activityPoint = unit.GetRegeneratedActionPoint();
			}
			apTextGO.GetComponent<CustomUIText>().text = activityPoint.ToString();

			if (unitWrapper.IsPreviewUnit())
			{
				GameObject arrow = profileGO.transform.Find("arrow").gameObject;
				arrow.GetComponent<Image>().enabled = true;
			}
		}

		private Sprite FindUnitProfileImage(Unit unit)
		{
			string imagePath = "UnitImage/portrait_" + unit.GetNameInCode().ToString();

			Sprite sprite = Resources.Load(imagePath, typeof(Sprite)) as Sprite;
			if (sprite == null)
			{
				Debug.LogWarning("Cannot find sprite for " + unit.GetName() + "(" + unit.GetNameInCode() + ")");
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
	}
}
