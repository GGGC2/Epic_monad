using UnityEngine;
using Battle.Damage;

namespace Battle.Skills
{
public class Yeong_0_1_SkillLogic : BasePassiveSkillLogic {

	public override void triggerActionEnd(Unit caster)
	{
		Vector2 currentPosition = caster.GetPosition();
		Vector2 startPosition = caster.GetStartPositionOfPhase();
		int deltaDistance = Utility.GetDistance(currentPosition, startPosition);
		Debug.Log("DD : " + deltaDistance);
		if (deltaDistance > 2)
			StatusEffector.AttachStatusEffect(caster, this.passiveSkill, caster);
	}
}
}
