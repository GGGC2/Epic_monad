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

	    public override IEnumerator TriggerOnKill(HitInfo hitInfo, Unit deadUnit)
	    {
		    foreach (var skillLogic in passiveSkillLogics)
		    {
			    yield return skillLogic.TriggerOnKill(hitInfo, deadUnit);
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

        public override float ApplyIgnoreResistanceRelativeValueByEachPassive(ActiveSkill appliedSkill, Unit target, Unit caster, float resistance) {
            foreach (var skillLogic in passiveSkillLogics) {
                resistance = skillLogic.ApplyIgnoreResistanceRelativeValueByEachPassive(appliedSkill, target, caster, resistance);
            }
            return resistance;
        }

        public override float ApplyIgnoreResistanceAbsoluteValueByEachPassive(ActiveSkill appliedSkill, Unit target, Unit caster, float resistance) {
            foreach (var skillLogic in passiveSkillLogics) {
                resistance = skillLogic.ApplyIgnoreResistanceAbsoluteValueByEachPassive(appliedSkill, target, caster, resistance);
            }
            return resistance;
        }

        public override float ApplyIgnoreDefenceRelativeValueByEachPassive(ActiveSkill appliedSkill, Unit target, Unit caster, float defense) {
            foreach (var skillLogic in passiveSkillLogics) {
                defense = skillLogic.ApplyIgnoreDefenceRelativeValueByEachPassive(appliedSkill, target, caster, defense);
            }
            return defense;
        }

        public override float ApplyIgnoreDefenceAbsoluteValueByEachPassive(ActiveSkill appliedSkill, Unit target, Unit caster, float defense) {
            foreach (var skillLogic in passiveSkillLogics) {
                defense = skillLogic.ApplyIgnoreDefenceAbsoluteValueByEachPassive(appliedSkill, target, caster, defense);
            }
            return defense;
        }

        public override void ApplyBonusDamageFromEachPassive(SkillInstanceData skillInstanceData) {
            float originalRelativeDamageBonus = skillInstanceData.GetDamage().relativeDamageBonus;
            float originalAbsoluteDamageBonus = skillInstanceData.GetDamage().absoluteDamageBonus;
            foreach (var skillLogic in passiveSkillLogics) {
                skillLogic.ApplyBonusDamageFromEachPassive(skillInstanceData);
                DamageCalculator.printBonusDamageLog(skillInstanceData.GetDamage(), originalAbsoluteDamageBonus, originalRelativeDamageBonus, skillLogic.passiveSkill.GetName());
                originalRelativeDamageBonus = skillInstanceData.GetDamage().relativeDamageBonus;
                originalAbsoluteDamageBonus = skillInstanceData.GetDamage().absoluteDamageBonus;
            }
        }

        public override void ApplyTacticalBonusFromEachPassive(SkillInstanceData skillInstanceData) {
            float originalRelativeDamageBonus = skillInstanceData.GetDamage().relativeDamageBonus;
            float originalAbsoluteDamageBonus = skillInstanceData.GetDamage().absoluteDamageBonus;
            foreach (var skillLogic in passiveSkillLogics) {
                skillLogic.ApplyTacticalBonusFromEachPassive(skillInstanceData);
                DamageCalculator.printBonusDamageLog(skillInstanceData.GetDamage(), originalAbsoluteDamageBonus, originalRelativeDamageBonus, skillLogic.passiveSkill.GetName());
                originalRelativeDamageBonus = skillInstanceData.GetDamage().relativeDamageBonus;
                originalAbsoluteDamageBonus = skillInstanceData.GetDamage().absoluteDamageBonus;
            }
        }

        public override IEnumerator ActionInDamageRoutine(SkillInstanceData skillInstanceData) {
            foreach(var skillLogic in passiveSkillLogics) {
                yield return skillLogic.ActionInDamageRoutine(skillInstanceData);
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

        public override bool TriggerDamaged(Unit target, float damage, Unit caster, bool isSourceTrap) {
        bool ignored = false;
            foreach (var skillLogic in passiveSkillLogics) {
                if (!skillLogic.TriggerDamaged(target, damage, caster, isSourceTrap)) {
                    ignored = true;
                }
            }
            return !ignored;
        }

        public override void TriggerAfterDamaged(Unit target, int damage, Unit caster) {
            foreach (var skillLogic in passiveSkillLogics) {
                skillLogic.TriggerAfterDamaged(target, damage, caster);
            }
        }

        public override bool TriggerStatusEffectAppliedToOwner(StatusEffect statusEffect, Unit caster, Unit target) {
            bool ignored = false;
            foreach (var skillLogic in passiveSkillLogics) {
                if (!skillLogic.TriggerStatusEffectAppliedToOwner(statusEffect, caster, target)) {
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
        public override IEnumerator TriggerWhenShieldWhoseCasterIsOwnerIsAttacked(Unit attacker, Unit shieldCaster, Unit target, float amount) {
            foreach(var skillLogic in passiveSkillLogics) {
                yield return skillLogic.TriggerWhenShieldWhoseCasterIsOwnerIsAttacked(attacker, shieldCaster, target, amount);
            }
        }
        public override void TriggerOnMove(Unit caster) {
            foreach (var skillLogic in passiveSkillLogics) {
                skillLogic.TriggerOnMove(caster);
            }
        }
        public override bool TriggerOnForceMove(Unit caster, Tile tileAfter) {
            bool ignored = false;
            foreach (var skillLogic in passiveSkillLogics) {
                if (!skillLogic.TriggerOnForceMove(caster, tileAfter)) {
                    ignored = true;
                }
            }
            return !ignored;
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

        public override IEnumerator TriggerOnPhaseStart(Unit caster, int phase) {		
		    foreach (var skillLogic in passiveSkillLogics)
		    {
			    yield return skillLogic.TriggerOnPhaseStart(caster, phase);
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
        public override bool TriggerOnSteppingTrap(Unit caster, Tile tile, TileStatusEffect trap) {
            bool ignored = false;
            foreach (var skillLogic in passiveSkillLogics) {
                if (!skillLogic.TriggerOnSteppingTrap(caster, tile, trap)) {
                    ignored = true;
                }
            }
            return !ignored;
        }
    }
}
