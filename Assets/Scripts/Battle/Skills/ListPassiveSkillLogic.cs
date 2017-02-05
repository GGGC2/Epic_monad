using System.Collections.Generic;

namespace Battle.Skills
{
public class ListPassiveSkillLogic : BasePassiveSkillLogic
{
	List<BasePassiveSkillLogic> passiveSkills;

	public ListPassiveSkillLogic(List<BasePassiveSkillLogic> passiveSkills)
	{
		this.passiveSkills = passiveSkills;
	}

	public override bool checkEvade()
	{
		foreach (var skill in passiveSkills)
		{
			if (skill.checkEvade())
			{
				return true;
			}
		}

		return false;
	}
}
}
