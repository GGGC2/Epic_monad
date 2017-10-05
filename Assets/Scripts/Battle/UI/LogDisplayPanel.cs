using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LogDisplayPanel : MonoBehaviour {
    public GameObject scrollView;
    public Scrollbar scrollbar;
    public VerticalLayoutGroup logs;
    public readonly int displayHeight = 52;
    int maxNumDisplay;

    public void Awake() {
        //scrollView = GetComponentInChildren<ScrollRect>().gameObject;
        //scrollbar = scrollView.GetComponentInChildren<Scrollbar>();
        //logs = scrollView.GetComponentInChildren<VerticalLayoutGroup>();
        CalculateMaxNumDisplay();
    }

    public void CalculateMaxNumDisplay() {
        maxNumDisplay = (int)(GetComponent<RectTransform>().sizeDelta.y / displayHeight);
    }

    public void AddLogDisplay(LogDisplay logDisplay, int num) {
        logDisplay.gameObject.transform.SetParent(logs.gameObject.transform, false);
        logDisplay.name = "Log " + num;
        logDisplay.SetText();
    }
    public void Initialize() {
        scrollbar.numberOfSteps = BattleData.logDisplayList.Count - maxNumDisplay + 1;
        scrollbar.value = 0;
    }
}