using System.Collections.Generic;
using UnityEngine;

namespace BattleUI.APBarPannels
{
	public class UnitWrapper
	{
		GameObject unitGO = null;
		bool isPreview = false;

		public UnitWrapper(GameObject unit, bool isPreview)
		{
			this.unitGO = unit;
			this.isPreview = isPreview;
		}

		public Unit GetUnit()
		{
			return unitGO.GetComponent<Unit>();
		}

		public GameObject GetGameObject()
		{
			return unitGO;
		}

		// Preview unit has indication arrow, So distinguish from others
		public bool IsPreviewUnit()
		{
			return isPreview;
		}
	}

	public class UnitWrapperFactory
	{
		GameObject selectedUnit = null;

		public UnitWrapperFactory(GameObject selectedUnit)
		{
			this.selectedUnit = selectedUnit;
		}

		public List<UnitWrapper> WrapUnits(List<GameObject> units)
		{
			List<UnitWrapper> wrappedUnits = new List<UnitWrapper>();
			foreach (GameObject unit in units)
			{
				wrappedUnits.Add(new UnitWrapper(unit, unit == selectedUnit));
			}
			return wrappedUnits;
		}
	}
}
