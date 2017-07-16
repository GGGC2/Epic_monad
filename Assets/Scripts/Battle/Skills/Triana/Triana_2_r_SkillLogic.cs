using UnityEngine;
using System.Linq;

namespace Battle.Skills
{
public class Triana_2_r_SkillLogic : BasePassiveSkillLogic {

	public override float GetAdditionalAbsoluteDefenseBonus(Unit caster)
	{
		float bonusAmountPerLevel = 0.7f;
		float baseAmountPerLevel = 41;

		if (caster.GetElement() == Enums.Element.Plant)
			return bonusAmountPerLevel + (baseAmountPerLevel * GameData.PartyData.level);
		else
			return 0;
	}
}
}
