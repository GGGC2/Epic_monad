using System.Collections.Generic;

namespace Save
{
public class PartyDB
{
	public static int GetPartyLevel()
	{
		return SaveDataCenter.GetSaveData().party.partyLevel;
	}

	public static List<string> GetPartyUnits()
	{
		return SaveDataCenter.GetSaveData().party.partyUnitNames;
	}

	public static void AddPartyUnit(string unitName)
	{
		List <string> partyUnits = SaveDataCenter.GetSaveData().party.partyUnitNames;
		partyUnits.Add(unitName);
		SaveDataCenter.Save();
	}
}
}
