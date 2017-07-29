using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

public class Skill{
    //기본 정보
    public string owner;
	public int column;
	public string korName;
	public int requireLevel;

    //유저에게 보여질 설명 텍스트 구성
    public string skillDataText;
	public Stat firstTextValueType;
	public float firstTextValueCoef;
	public Stat secondTextValueType;
	public float secondTextValueCoef;

    //기술,특성의 공통되는 부분을 받아온다
    public void GetCommonSkillData(CommaStringParser parser){
        owner = parser.Consume();
		requireLevel = parser.ConsumeInt();
		korName = parser.Consume();
		column = parser.ConsumeInt();
    }
}