using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Setting{
	//기능 통제
	public static readonly int classOpenStage = 2;
	public static readonly int mouseCameraMoveOpenStage = 2;
	public static readonly int directionOpenStage = 2;
	public static readonly int elementOpenStage = 3;
	public static readonly int passiveOpenStage = 3;
	public static readonly int chainOpenStage = 4;
	public static readonly int celestialOpenStage = 4;
	public static readonly int retreatOpenStage = 4;
	public static readonly int heightOpenStage = 7;
	public static readonly int readySceneOpenStage = 8; //능력 선택과 동시에 개방
	public static readonly int unitSelectOpenStage = 12;
	public static bool shortcutEnable = true;

	//변수 통제
	public static readonly float retreatHPFloat = 0.2f;
	public static readonly float fadeInWaitingTime = 0.3f;
	public static readonly float tileImageHeight = 0.5f;
	public static readonly float tileImageWidth = 1.0f;
	public static readonly int moveCostAcc = 2; //이동할 때 타일당 추가로 붙는 계차값
	public static readonly int fastDialogueFrameLag = 4; //대화 모드에서 빨리 넘길 때(CTRL) 몇 프레임마다 한 번씩 넘길지 설정
	public static readonly float sideAttackBonus = 1.1f;
	public static readonly float backAttackBonus = 1.25f;
}