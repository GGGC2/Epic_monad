using System.Collections.Generic;
using System.Collections;

namespace Battle.Skills
{
    public class ListPassiveSkillLogic : BasePassiveSkillLogic
    {
	    List<BasePassiveSkillLogic> passiveSkillLogics;

	    public ListPassiveSkillLogic(List<BasePassiveSkillLogic> passiveSkillLogics)
	    {
		    this.passiveSkillLogics = passiveSkillLogics;
	    }

	    public override void ApplyStatusEffectByKill(HitInfo hitInfo, Unit deadUnit)
	    {
		    foreach (var skillLogic in passiveSkillLogics)
		    {
			    skillLogic.ApplyStatusEffectByKill(hitInfo, deadUnit);
		    }
	    }

        public override float GetAdditionalRelativePowerBonus(Unit caster) {
            float totalAdditionalPowerBonus = 1.0f;
            foreach (var skillLogic in passiveSkillLogics) {
                totalAdditionalPowerBonus *= skillLogic.GetAdditionalRelativePowerBonus(caster);
            }

            return totalAdditionalPowerBonus;
        }

        public override float GetAdditionalAbsoluteDefenseBonus(Unit caster) {
            float totalAdditionalDefenseBonus = 0;
            foreach (var skillLogic in passiveSkillLogics) {
                totalAdditionalDefenseBonus += skillLogic.GetAdditionalAbsoluteDefenseBonus(caster);
            }
            return totalAdditionalDefenseBonus;
        }

        public override float ApplyIgnoreResistanceRelativeValueByEachPassive(SkillInstanceData skillInstanceData, float resistance) {
            foreach (var skillLogic in passiveSkillLogics) {
                resistance = skillLogic.ApplyIgnoreResistanceRelativeValueByEachPassive(skillInstanceData, resistance);
            }
            return resistance;
        }

        public override float ApplyIgnoreResistanceAbsoluteValueByEachPassive(SkillInstanceData skillInstanceData, float resistance) {
            foreach (var skillLogic in passiveSkillLogics) {
                resistance = skillLogic.ApplyIgnoreResistanceAbsoluteValueByEachPassive(skillInstanceData, resistance);
            }
            return resistance;
        }

        public override float ApplyIgnoreDefenceRelativeValueByEachPassive(SkillInstanceData skillInstanceData, float defense) {
            foreach (var skillLogic in passiveSkillLogics) {
                defense = skillLogic.ApplyIgnoreDefenceRelativeValueByEachPassive(skillInstanceData, defense);
            }
            return defense;
        }

        public override float ApplyIgnoreDefenceAbsoluteValueByEachPassive(SkillInstanceData skillInstanceData, float defense) {
            foreach (var skillLogic in passiveSkillLogics) {
                defense = skillLogic.ApplyIgnoreDefenceAbsoluteValueByEachPassive(skillInstanceData, defense);
            }
            return defense;
        }

        public override void ApplyBonusDamageFromEachPassive(SkillInstanceData skillInstanceData) {
            foreach (var skillLogic in passiveSkillLogics) {
                skillLogic.ApplyBonusDamageFromEachPassive(skillInstanceData);
            }
        }

        public override void ApplyTacticalBonusFromEachPassive(SkillInstanceData skillInstanceData) {
            foreach (var skillLogic in passiveSkillLogics) {
                skillLogic.ApplyTacticalBonusFromEachPassive(skillInstanceData);
            }
        }


        public override int GetEvasionChance()
	    {
		    int totalEvasionChance = 0;
		    foreach (var skillLogic in passiveSkillLogics)
		    {
			    totalEvasionChance += skillLogic.GetEvasionChance();
		    }

		    return totalEvasionChance;
	    }
        public override float ApplyAdditionalRecoverHealthDuringRest(Unit caster, float baseAmount) {
            foreach (var skillLogic in passiveSkillLogics) {
                baseAmount = skillLogic.ApplyAdditionalRecoverHealthDuringRest(caster, baseAmount);
            }
            return baseAmount;
        }

        public override void TriggerOnEvasionEvent(BattleData battleData, Unit caster, Unit target)
	    {
		    foreach (var skillLogic in passiveSkillLogics)
		    {
			    skillLogic.TriggerOnEvasionEvent(battleData, caster, target);
		    }
	    }

	    public override void TriggerOffensiveActiveSkillApplied(Unit caster)
	    {
		    foreach (var skillLogic in passiveSkillLogics)
		    {
			    skillLogic.TriggerOffensiveActiveSkillApplied(caster);
		    }
	    }

        public override void TriggerActiveSkillDamageApplied(Unit caster, Unit target) {
            foreach (var skillLogic in passiveSkillLogics) {
                skillLogic.TriggerActiveSkillDamageApplied(caster, target);
            }
        }
    
        public override void TriggerDamaged(Unit target, int damage, Unit caster) {
            foreach (var skillLogic in passiveSkillLogics) {
                skillLogic.TriggerDamaged(target, damage, caster);
            }
        }

        public override bool TriggerStatusEffectApplied(StatusEffect statusEffect, Unit caster, Unit target) {
            bool ignored = false;
            foreach (var skillLogic in passiveSkillLogics) {
                if (!skillLogic.TriggerStatusEffectApplied(statusEffect, caster, target)) {
                    ignored = true;
                }
            }
            return !ignored;
        }

        public override bool TriggerStatusEffectRemoved(StatusEffect statusEffect, Unit unit) {
            bool ignored = false;
            foreach (var skillLogic in passiveSkillLogics) {
                if (!skillLogic.TriggerStatusEffectRemoved(statusEffect, unit)) {
                    ignored = true;
                }
            }
            return !ignored;
        }

        public override void TriggerUsingSkill(Unit caster, List<Unit> targets)
	    {
		    foreach (var skillLogic in passiveSkillLogics)
		    {
			    skillLogic.TriggerUsingSkill(caster, targets);
		    }
	    }
        public override void TriggerOnMove(Unit caster) {
            foreach (var skillLogic in passiveSkillLogics) {
                skillLogic.TriggerOnMove(caster);
            }
        }

        public override IEnumerator TriggerApplyingHeal(SkillInstanceData skillInstanceData) {
            foreach (var skillLogic in passiveSkillLogics) {
                yield return skillLogic.TriggerApplyingHeal(skillInstanceData);
            }
        }

        public override void TriggerOnStart(Unit caster) {
            foreach(var skillLogic in passiveSkillLogics) {
                skillLogic.TriggerOnStart(caster);
            }
        }

        public override IEnumerator TriggerOnPhaseStart(Unit caster) {		
		    foreach (var skillLogic in passiveSkillLogics)
		    {
			    yield return skillLogic.TriggerOnPhaseStart(caster);
		    }
	    }

        public override void TriggerOnPhaseEnd(Unit caster) {
            foreach (var skillLogic in passiveSkillLogics) {
                skillLogic.TriggerOnPhaseEnd(caster);
            }
        }

        public override void TriggerOnActionEnd(Unit caster) {
            foreach (var skillLogic in passiveSkillLogics) {
                skillLogic.TriggerOnActionEnd(caster);
            }
        }

        public override void TriggerOnRest(Unit caster) {
            foreach(var skillLogic in passiveSkillLogics) {
                skillLogic.TriggerOnRest(caster);
            }
        }
        public override void TriggerOnTurnStart(Unit caster, Unit turnStarter) {
            foreach (var skillLogic in passiveSkillLogics) {
                skillLogic.TriggerOnTurnStart(caster, turnStarter);
            }
        }
        public override void TriggerStatusEffectsOnRest(Unit target, StatusEffect statusEffect) {
            foreach (var skillLogic in passiveSkillLogics) {
                skillLogic.TriggerStatusEffectsOnRest(target, statusEffect);
            }
        }
        public override void TriggerStatusEffectsOnUsingSkill(Unit target, List<Unit> targetsOfSkill, StatusEffect statusEffect) {
            foreach (var skillLogic in passiveSkillLogics) {
                skillLogic.TriggerStatusEffectsOnUsingSkill(target, targetsOfSkill, statusEffect);
            }
        }
        public override void TriggerStatusEffectsOnMove(Unit target, StatusEffect statusEffect) {
            foreach (var skillLogic in passiveSkillLogics) {
                skillLogic.TriggerStatusEffectsOnMove(target, statusEffect);
            }
        }
    }
}
