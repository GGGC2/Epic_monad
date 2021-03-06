﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

public class Skill{
    //기본 정보
    public string owner;
    public int ether;
	public int row;
	public string korName;
	public int requireLevel;

    //유저에게 보여질 설명 텍스트 구성
    public string skillDataText;
	public Stat firstTextValueType;
	public float firstTextValueCoef;
    public float firstTextValueBase;
	public Stat secondTextValueType;
	public float secondTextValueCoef;
    public float secondTextValueBase;
    public Sprite icon;

    //기술,특성의 공통되는 부분을 받아온다
    public void GetCommonSkillData(StringParser parser){
        owner = parser.Consume();
        ether = parser.ConsumeInt();
		requireLevel = parser.ConsumeInt();
		korName = parser.Consume();
		row = parser.ConsumeInt();
        icon = Utility.SkillIconOf(owner, requireLevel, row);
    }

    public static Skill Find(List<Skill> list, string owner, int level, int row){
        return list.Find(skill => skill.owner == owner && skill.requireLevel == level && skill.row == row);
    }
}