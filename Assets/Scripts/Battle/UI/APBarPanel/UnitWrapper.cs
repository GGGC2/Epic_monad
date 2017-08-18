using System.Collections.Generic;
using UnityEngine;

namespace BattleUI.APBarPanels{
	public class UnitWrapper{
		Unit unit = null;
		bool isPreview = false;

		public UnitWrapper(Unit unit, bool isPreview){
			this.unit = unit;
			this.isPreview = isPreview;
		}

		public Unit GetUnit() {return unit;}
		public GameObject GetGameObject() {return unit.gameObject;}

		// Preview unit has indication arrow, So distinguish from others
		public bool IsPreviewUnit() {return isPreview;}
	}

	public class UnitWrapperFactory{
		Unit selectedUnit = null;

		public UnitWrapperFactory(Unit selectedUnit)
		{
			this.selectedUnit = selectedUnit;
		}

		public List<UnitWrapper> WrapUnits(List<Unit> units){
			List<UnitWrapper> wrappedUnits = new List<UnitWrapper>();
			foreach (Unit unit in units)
			{
				wrappedUnits.Add(new UnitWrapper(unit, unit == selectedUnit));
			}
			return wrappedUnits;
		}
	}
}
