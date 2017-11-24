using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour{
	//유니티 버튼 함수는 parameter를 하나밖에 줄 수 없어서 함수를 쪼개놓음
	public void GoToTitleScene(){
		SceneManager.LoadScene("Title");
	}

    public void RestartBattle() {
        FindObjectOfType<SceneLoader>().LoadNextBattleScene();
    }
}
