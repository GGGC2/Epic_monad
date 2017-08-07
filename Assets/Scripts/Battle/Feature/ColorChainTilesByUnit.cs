using System.Collections.Generic;
using UnityEngine;

using Enums;

namespace Battle.Feature
{
	public class ColorChainTilesByUnit
	{
		public static void Show(Unit unit)
		{
			BattleManager battleManager = GameObject.FindObjectOfType<BattleManager>();
			BattleData battleData = battleManager.battleData;

			Chain chain = battleData.GetChain(unit);
			if (chain == null)
			{
				return;
			}

			List<Tile> targetArea = chain.GetSecondRange();
			battleData.tileManager.PaintTiles(targetArea, TileColor.Yellow);
		}

		public static void Hide(Unit unit)
		{
			BattleManager battleManager = GameObject.FindObjectOfType<BattleManager>();
			BattleData battleData = battleManager.battleData;

			Chain chain = battleData.GetChain(unit);
			if (chain == null)
			{
				return;
			}

			List<Tile> targetArea = chain.GetSecondRange();
			battleData.tileManager.DepaintTiles(targetArea, TileColor.Yellow);
		}
	}
}
