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

			ChainInfo chainInfo = battleData.GetChainInfo(unit);
			if (chainInfo == null)
			{
				return;
			}

			List<Tile> targetArea = chainInfo.GetSecondRange();
			battleData.tileManager.PaintTiles(targetArea, TileColor.Yellow);
		}

		public static void Hide(Unit unit)
		{
			BattleManager battleManager = GameObject.FindObjectOfType<BattleManager>();
			BattleData battleData = battleManager.battleData;

			ChainInfo chainInfo = battleData.GetChainInfo(unit);
			if (chainInfo == null)
			{
				return;
			}

			List<Tile> targetArea = chainInfo.GetSecondRange();
			battleData.tileManager.DepaintTiles(targetArea, TileColor.Yellow);
		}
	}
}
