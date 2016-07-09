using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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

			SetBigProfile(allUnits[0], standardActionPoint);

			List<GameObject> otherUnits = allUnits.GetRange(index:1, count:allUnits.Count - 1);
			SetProfiles(standardActionPoint, otherUnits);

			SetCurrentText(standardActionPoint, otherUnits);
			SetNextText(standardActionPoint, otherUnits);
			SetSeperateBar(standardActionPoint, otherUnits);
		}

		private void SetBigProfile(GameObject unitGO, int standardActionPoint)
		{
			bigProfile.GetComponent<Image>().enabled = true;
			SetProfile(bigProfile, unitGO, standardActionPoint);
		}

		private void SetProfiles(int standardActionPoint, List<GameObject> otherUnitGOs)
		{
			if (otherUnitGOs.Count == 0)
			{
				return;
			}

			for (int index = 0; index < otherUnitGOs.Count; index += 1)
			{
				GameObject profileGameObject = otherProfiles[index];
				GameObject unit = otherUnitGOs[index];

				profileGameObject.GetComponent<Image>().enabled = true;
				SetProfile(profileGameObject, unit, standardActionPoint);
			}

			// Does not make space no units left in current turn.
			if (otherUnitGOs[0].GetComponent<Unit>().GetCurrentActivityPoint() < standardActionPoint)
			{
				return;
			}

			for (int index = 0; index < otherUnitGOs.Count; index += 1)
			{
				GameObject profileGameObject = otherProfiles[index];
				GameObject unit = otherUnitGOs[index];

				if (unit.GetComponent<Unit>().GetCurrentActivityPoint() < standardActionPoint)
				{
					profileGameObject.GetComponent<RectTransform>().anchoredPosition += new Vector2(seperationSpace * 2, 0);
				}
			}
		}

		private void SetCurrentText(int standardActionPoint, List<GameObject> otherUnitGOs)
		{
			if (otherUnitGOs.Count == 0)
			{
				return;
			}

			Unit firstUnit = otherUnitGOs[0].GetComponent<Unit>();
			if (firstUnit.GetCurrentActivityPoint() >= standardActionPoint)
			{
				currentTurnTexts[0].GetComponent<Image>().enabled = true;
			}
			else
			{
				return;
			}
		}

		private void SetNextText(int standardActionPoint, List<GameObject> otherUnitGOs)
		{
			for (int i=0; i < otherUnitGOs.Count; i+=1)
			{
				Unit unit = otherUnitGOs[i].GetComponent<Unit>();
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

		private void SetSeperateBar(int standardActionPoint, List<GameObject> otherUnitGOs)
		{
			int nextTurnUnitIndex = -1;
			for (int i=0; i < otherUnitGOs.Count; i+=1)
			{
				Unit unit = otherUnitGOs[i].GetComponent<Unit>();
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

		private void SetProfile(GameObject profileGO, GameObject unitGO, int standardActionPoint)
		{
			Unit unit = unitGO.GetComponent<Unit>();
			string imagePath = "UnitImage/portrait_" + unit.GetNameInCode().ToString();

			Sprite sprite = Resources.Load(imagePath, typeof(Sprite)) as Sprite;
			if (sprite == null)
			{
				Debug.LogWarning("Cannot find sprite for " + unit.GetName() + "(" + unit.GetNameInCode() + ")");
				sprite = notFoundProfileSprite;
			}
			profileGO.GetComponent<Image>().sprite = sprite;

			Debug.Log("Finding apText in " + profileGO.name);
			GameObject apTextGO = profileGO.transform.Find("apText").gameObject;
			apTextGO.GetComponent<CustomUIText>().enabled = true;
			int activityPoint = unit.activityPoint;
			if (activityPoint < standardActionPoint)
			{
				activityPoint = unit.GetRegeneratedActionPoint();
			}
			apTextGO.GetComponent<CustomUIText>().text = activityPoint.ToString();
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
