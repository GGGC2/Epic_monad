using UnityEngine;

namespace Battle.Skills
{
public class Yeong_0_1_SkillLogic : BasePassiveSkillLogic {

	public override bool checkEvade()
	{
		return Random.Range(0.0f, 1.0f) < 0.2;
	}
}
}
