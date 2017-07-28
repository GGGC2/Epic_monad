﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Calculator{
	public static string GetSkillBasePower(int power, Skill skill){
		return ((int)((float)power*skill.GetPowerFactor(Enums.Stat.Power))).ToString();
	}

	public static string GetSkillBasePower(string powerString, Skill skill){
		return ((int)((float)Int32.Parse(powerString)*skill.GetPowerFactor(Enums.Stat.Power))).ToString();
	}
}