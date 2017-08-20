using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Setting{
	//기능 통제
	public static int classOpenStage = 2;
	public static int elementOpenStage = 3;
	public static int passiveOpenStage = 3;
	public static int directionOpenStage = 1;
	public static int chainOpenStage = 2;
	public static int celestialOpenStage = 4;
	public static int retreatOpenStage = 4;
	public static int readySceneOpenStage = 5;
	public static bool shortcutEnable = true;
	public static int mouseCameraMoveOpenStage = 2;

	//변수 통제
	public static int retreatHpPercent = 10;
	public static float retreatHpFloat = 0.1f;
	public static float fadeInWaitingTime = 0.3f;
	//이동할 때 타일당 추가로 붙는 계차값
	public static int moveCostAcc = 2;
}