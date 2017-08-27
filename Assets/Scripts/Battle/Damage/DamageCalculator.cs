using System;
using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine;
using Battle.Skills;
using GameData;

namespace Battle
{
	public class DamageCalculator
	{
		public class AttackDamage {
			public float baseDamage = 0;
			public float relativeDamageBonus = 1.0f;
			public float absoluteDamageBonus = 0;
			public DirectionCategory attackDirection = DirectionCategory.Front;
			public float directionBonus = 1.0f;
			public float celestialBonus = 1.0f;
			public float heightBonus = 1.0f;
			public float chainBonus = 1.0f;
			public float smiteAmount = 0;
			public float resultDamage = 0;
		}

		public class DamageInfo{
			public List<Unit> casters;
			public float damage;

			public DamageInfo(Unit caster, float damage){
				this.casters = new List<Unit>();
				casters.Add(caster);
				this.damage = damage;
			}
		}

		public static Dictionary<Unit, DamageInfo> CalculateAllPreviewTotalDamages(Casting casting){
			Dictionary<Unit, DamageInfo> damageList = new Dictionary<Unit, DamageInfo>();

			List<Chain> allChainTriggered = ChainList.GetAllChainTriggered (casting);
			int chainCombo = allChainTriggered.Count;
			foreach (var chain in allChainTriggered)
			{
				var damageListOfEachSkill = CalculatePreviewDamageOfEachChain(chain, chainCombo);
				damageList = MergeDamageList(damageList, damageListOfEachSkill);
			}

			return damageList;
		}

		private static Dictionary<Unit, DamageInfo> MergeDamageList(Dictionary<Unit, DamageInfo> leftDamgeList, Dictionary<Unit, DamageInfo> rightDamageList){
			var merged = new Dictionary<Unit, DamageInfo>();
			foreach (var damage in leftDamgeList)
			{
				var target = damage.Key;
				merged[target] = damage.Value;
			}
			foreach (var damage in rightDamageList)
			{
				var target = damage.Key;
				if (merged.ContainsKey(target))
				{
					foreach (var caster in rightDamageList[target].casters)
						merged[target].casters.Add(caster);
					merged[target].damage += rightDamageList[target].damage;
				}
				else
				{
					merged[target] = rightDamageList[target];
				}
			}
			return merged;
		}

		private static Dictionary<Unit, DamageInfo> CalculatePreviewDamageOfEachChain(Chain chain, int chainCombo){
			var damageList = new Dictionary<Unit, DamageInfo>();
			ActiveSkill appliedSkill = chain.Skill;
			Unit caster = chain.Caster;			
			List<Unit> targets = chain.CurrentTargets;

			foreach (var target in targets)
			{
				CastingApply castingApply = new CastingApply(chain.Casting, target);

				if (appliedSkill.GetSkillApplyType() == SkillApplyType.DamageHealth) {
					CalculateAttackDamage(castingApply, chainCombo);

					float damageBeforeReflection = castingApply.GetDamage().resultDamage;
					float reflectDamage = CalculateReflectDamage(damageBeforeReflection, target, caster, caster.GetUnitClass());

					float actualDamage = target.CalculateDamageByCasting (castingApply, true);

					DamageInfo damageInfo = new DamageInfo(caster, actualDamage);
					damageList.Add(target, damageInfo);

					//Debug.Log(actualDamage + " damage will be applied to " + target.GetName() + "\n" +
					//	"ChainCombo : " + chainCombo);

					if (reflectDamage != 0) {
						float reflectTargetDefense = CalculateDefense(appliedSkill, caster, target);
						float reflectTargetResistance = CalculateDefense(appliedSkill, caster, target);
						reflectDamage = ApplyDefenseAndResistance(reflectDamage, target.GetUnitClass(), reflectTargetDefense, reflectTargetResistance);

						DamageInfo reflectDamageInfo = new DamageInfo(target, reflectDamage);
						damageList.Add(caster, reflectDamageInfo);
						//Debug.Log(reflectDamage + " damage will be reflected from " + target.GetName() + " to " + caster.GetName());
					}
				}
				else if (appliedSkill.GetSkillApplyType() == SkillApplyType.HealHealth) {
					CalculateAmountOtherThanAttackDamage(castingApply);
					float actualHealAmount = castingApply.GetDamage().resultDamage;
					DamageInfo damageInfo = new DamageInfo(caster, -actualHealAmount);
					damageList.Add(target, damageInfo);

					//Debug.Log(actualHealAmount + " heal will be applied to " + target.GetName());
				}
			}
			return damageList;
		}

		private static List<Unit> GetTargetUnits(List<Tile> targetTiles) {
			var targets = new List<Unit>();
			foreach (var tile in targetTiles)
			{
				if (tile.IsUnitOnTile())
				{
					targets.Add(tile.GetUnitOnTile());
				}
			}
			return targets;
		}

		public static void CalculateAmountOtherThanAttackDamage(CastingApply castingApply) {
			Unit caster = castingApply.GetCaster();
			Unit target = castingApply.GetTarget();
			AttackDamage attackDamage = castingApply.GetDamage();
			ActiveSkill appliedSkill = castingApply.GetSkill();

			attackDamage.baseDamage = PowerFactorDamage(appliedSkill, caster);
			// 해당 기술의 추가데미지 계산
			SkillLogicFactory.Get(appliedSkill).ApplyAdditionalDamage(castingApply);
			foreach(var statusEffect in target.GetStatusEffectList()) {
				Skill originSkill = statusEffect.GetOriginSkill();
				if(originSkill.GetType() == typeof(ActiveSkill))
					((ActiveSkill)originSkill).SkillLogic.ApplyAdditionalDamageFromTargetStatusEffect(castingApply, statusEffect);
			}
			// 특성에 의한 추가데미지
			List<PassiveSkill> passiveSkills = caster.GetLearnedPassiveSkillList();
			SkillLogicFactory.Get(passiveSkills).ApplyBonusDamageFromEachPassive(castingApply);

			attackDamage.resultDamage = attackDamage.baseDamage * attackDamage.relativeDamageBonus;
			//Debug.Log("resultAmount : " + attackDamage.resultDamage);
		}
		public static void CalculateAttackDamage(CastingApply castingApply, int chainCombo){
			Unit caster = castingApply.GetCaster();
			Unit target = castingApply.GetTarget();
			AttackDamage attackDamage = castingApply.GetDamage();
			ActiveSkill appliedSkill = castingApply.GetSkill();

			attackDamage.baseDamage = PowerFactorDamage(appliedSkill, caster);
			attackDamage.directionBonus = DirectionBonus(caster, target);
			attackDamage.attackDirection = AttackDirection(caster, target);
			attackDamage.celestialBonus = CelestialBonus(caster, target);
			attackDamage.heightBonus = HeightBonus(caster, target);
			attackDamage.chainBonus = ChainComboBonus(chainCombo);
			attackDamage.smiteAmount = SmiteAmount(caster);

			Element casterElement = caster.GetElement();
			if(casterElement != Element.None){
				StatusEffectType casterElementWeakness = EnumConverter.GetCorrespondingStatusEffectType(casterElement);
				if (target.HasStatusEffect(casterElementWeakness)){
					float elementBonus = target.CalculateActualAmount(1, casterElementWeakness);
					//Debug.Log("\tElement bonus" + "(" + casterElement + ")" + " : " + elementBonus);
					attackDamage.relativeDamageBonus *= elementBonus;
				}
			}

			float originalRelativeDamageBonus = attackDamage.relativeDamageBonus;
			float originalAbsoluteDamageBonus = attackDamage.absoluteDamageBonus;

			// 해당 기술의 추가데미지 계산
			SkillLogicFactory.Get(appliedSkill).ApplyAdditionalDamage(castingApply);
			foreach (var statusEffect in target.GetStatusEffectList()) {
				Skill originSkill = statusEffect.GetOriginSkill();
				if (originSkill.GetType() == typeof(ActiveSkill))
                    ((ActiveSkill)originSkill).SkillLogic.ApplyAdditionalDamageFromTargetStatusEffect(castingApply, statusEffect);
			}
			printBonusDamageLog(attackDamage, originalAbsoluteDamageBonus, originalRelativeDamageBonus, castingApply.GetSkill().GetName());
			originalRelativeDamageBonus = castingApply.GetDamage().relativeDamageBonus;
			originalAbsoluteDamageBonus = castingApply.GetDamage().absoluteDamageBonus;

			// 특성에 의한 추가데미지
			List<PassiveSkill> passiveSkills = caster.GetLearnedPassiveSkillList();
			SkillLogicFactory.Get(passiveSkills).ApplyBonusDamageFromEachPassive(castingApply);
			// 특성에 의한 전략보너스 추가
			SkillLogicFactory.Get(passiveSkills).ApplyTacticalBonusFromEachPassive(castingApply);
			originalRelativeDamageBonus = castingApply.GetDamage().relativeDamageBonus;
			originalAbsoluteDamageBonus = castingApply.GetDamage().absoluteDamageBonus;

			// 시전자 효과에 의한 추가데미지
			attackDamage.baseDamage = caster.CalculateActualAmount(attackDamage.baseDamage, StatusEffectType.DamageChange);
			printBonusDamageLog(attackDamage, originalAbsoluteDamageBonus, originalRelativeDamageBonus, "buff from " + caster.GetNameEng());

			// '지형지물'은 방향 보너스를 받지 않음
			if (target.IsObject) attackDamage.directionBonus = 1.0f;

			attackDamage.resultDamage = ((attackDamage.baseDamage + attackDamage.smiteAmount)
				* attackDamage.relativeDamageBonus + attackDamage.absoluteDamageBonus) 
				* attackDamage.directionBonus
				* attackDamage.celestialBonus
				* attackDamage.heightBonus
				* attackDamage.chainBonus;

			//Debug.Log("resultDamage : " + attackDamage.resultDamage);
		}

		public static void printBonusDamageLog(AttackDamage damage, float originalAbsoluteDamageBonus, float originalRelativeDamageBonus, string damageSource) {
			if (damage.relativeDamageBonus != originalRelativeDamageBonus) {
				float bonus = damage.relativeDamageBonus / originalRelativeDamageBonus;
				//Debug.Log("\tRelative damage bonus from " + damageSource + " : " + bonus);
			}
			if (damage.absoluteDamageBonus != originalAbsoluteDamageBonus) {
				float bonus = damage.absoluteDamageBonus - originalAbsoluteDamageBonus;
				//Debug.Log("\tAbsolute damage bonus from " + damageSource + " : " + bonus);
			}
		}

		private static float PowerFactorDamage(ActiveSkill appliedSkill, Unit casterUnit)
		{
			float damage = 0;

			float powerFactor = appliedSkill.GetPowerFactor(Stat.Power);
			float powerStat = casterUnit.GetStat(Stat.Power);

			damage = powerFactor * powerStat;

			//Debug.Log("baseDamage : " + damage);

			return damage;
		}

		private static float DirectionBonus(Unit caster, Unit target) {
			if(SceneData.stageNumber < Setting.directionOpenStage)
				return 1.0f;
			float directionBonus = Utility.GetDirectionBonus(caster, target);
			//Debug.Log("\tdirectionBonus : " + directionBonus);
			return directionBonus;
		}

		private static DirectionCategory AttackDirection(Unit caster, Unit target)
		{
			float directionBonus = Utility.GetDirectionBonus(caster, target);
			if (directionBonus == 1.1f)
				return DirectionCategory.Side;
			else if (directionBonus == 1.25f)
				return DirectionCategory.Back;
			else
				return DirectionCategory.Front; 
		}

		private static float CelestialBonus(Unit caster, Unit target) {
			float celestialBonus = Utility.GetCelestialBonus(caster, target);
			//Debug.Log("\tcelestialBonus : " + celestialBonus);
			return celestialBonus;
		}

		private static float HeightBonus(Unit caster, Unit target) {
			float heightBonus = Utility.GetHeightBonus(caster, target);
			//Debug.Log("\theightBonus : " + heightBonus);
			return heightBonus;
		}

		private static float ChainComboBonus(int chainCombo) {
			float chainBonus = GetChainDamageFactorFromChainCombo(chainCombo);
			//Debug.Log("\tchainBonus : " + chainBonus);
			return chainBonus;
		}

		private static float GetChainDamageFactorFromChainCombo(int chainCombo)
		{
			if (chainCombo < 2)	return 1.0f;
			else if (chainCombo == 2) return 1.2f;
			else if (chainCombo == 3) return 1.5f;
			else if (chainCombo == 4) return 2.0f;
			else return 3.0f;  
		}

		private static float SmiteAmount(Unit casterUnit) {
			float smiteAmount = 0;
			smiteAmount = casterUnit.CalculateActualAmount(smiteAmount, StatusEffectType.Smite);
			//Debug.Log("\tsmiteAmount : " + smiteAmount);
			return smiteAmount;
		}

		public static float CalculateReflectDamage(float attackDamage, Unit target, Unit reflectTarget, UnitClass damageType)
		{
			float reflectAmount = 0;
			foreach (var statusEffect in target.GetStatusEffectList())
			{
				bool canReflect = statusEffect.IsOfType(StatusEffectType.Reflect) ||
					(statusEffect.IsOfType(StatusEffectType.MagicReflect) && damageType == UnitClass.Magic) ||
					(statusEffect.IsOfType(StatusEffectType.MeleeReflect) && damageType == UnitClass.Melee);
				if (canReflect)
				{
					float reflectPercent = statusEffect.GetAmountOfType(StatusEffectType.Reflect);
					if(damageType == UnitClass.Magic) reflectPercent += statusEffect.GetAmountOfType(StatusEffectType.MagicReflect);
					if(damageType == UnitClass.Melee) reflectPercent += statusEffect.GetAmountOfType(StatusEffectType.MeleeReflect);
					reflectAmount += attackDamage * reflectPercent/100;
				}
			}
			return reflectAmount;
		}

		public static float ApplyDefenseAndResistance(float damage, UnitClass damageType, float defense, float resistance) {
			if(SceneData.stageNumber < Setting.classOpenStage){
				return damage;
			}
			if (damageType == UnitClass.Melee) {
				// 실제 피해 = 원래 피해 x 200/(200+방어력)
				// 방어력이 -180 이하일 시 -180으로 적용
				if (defense <= -180) damage = damage * 10;
				else damage = damage * 200.0f / (200.0f + defense);
			} else if (damageType == UnitClass.Magic) {
				if (resistance <= -180) damage = damage * 10;
				else damage = damage * 200.0f / (200.0f + resistance);
			}
			//Debug.Log("resultDamage applying defense and resistance applied : " + damage);
			return damage;
		}
		public static float CalculateDefense(ActiveSkill appliedSkill, Unit target, Unit caster) {
			float defense = target.GetStat(Stat.Defense);

			// 기술에 의한 방어 무시 (상대값)
			defense = SkillLogicFactory.Get(appliedSkill).ApplyIgnoreDefenceRelativeValueBySkill(defense, caster, target);

			// 특성에 의한 방어 무시 (상대값)
			List<PassiveSkill> casterPassiveSkills = caster.GetLearnedPassiveSkillList();
			defense = SkillLogicFactory.Get(casterPassiveSkills).ApplyIgnoreDefenceRelativeValueByEachPassive(appliedSkill, target, caster, defense); 

			// 기술에 의한 방어 무시 (절대값)
			defense = SkillLogicFactory.Get(appliedSkill).ApplyIgnoreDefenceAbsoluteValueBySkill(defense, caster, target);

			// 특성에 의한 방어 무시 (절대값)
			defense = SkillLogicFactory.Get(casterPassiveSkills).ApplyIgnoreDefenceAbsoluteValueByEachPassive(appliedSkill, target, caster, defense);
			return defense;
		}
		public static float CalculateResistance(ActiveSkill appliedSkill, Unit target, Unit caster) {
			float resistance = target.GetStat(Stat.Resistance);

			// 기술에 의한 저항 무시 (상대값)
			resistance = SkillLogicFactory.Get(appliedSkill).ApplyIgnoreResistanceRelativeValueBySkill(resistance, caster, target);

			// 특성에 의한 저항 무시 (상대값)
			List<PassiveSkill> casterPassiveSkills = caster.GetLearnedPassiveSkillList();
			resistance = SkillLogicFactory.Get(casterPassiveSkills).ApplyIgnoreResistanceRelativeValueByEachPassive(appliedSkill, target, caster, resistance);

			// 기술에 의한 저항 무시 (절대값)
			resistance = SkillLogicFactory.Get(appliedSkill).ApplyIgnoreResistanceAbsoluteValueBySkill(resistance, caster, target);

			// 특성에 의한 저항 무시 (절대값)
			resistance = SkillLogicFactory.Get(casterPassiveSkills).ApplyIgnoreResistanceAbsoluteValueByEachPassive(appliedSkill, target, caster, resistance);
			return resistance;
		}
	}
}
