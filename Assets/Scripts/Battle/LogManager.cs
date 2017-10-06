using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class LogManager : MonoBehaviour {
    public LogDisplayPanel logDisplayPanel;
    public GameObject logDisplayPrefab;
    private static LogManager instance = null;
    public static LogManager Instance { get { return instance; } }
    public static void SetInstance() { instance = FindObjectOfType<LogManager>(); }

    public void Awake() {
        logDisplayPanel = GameObject.Find("LogDisplayPanel").GetComponentInChildren<LogDisplayPanel>();
    }

    public void Record(Log log) {
        LogDisplay logDisplay = Instantiate(logDisplayPrefab).GetComponent<LogDisplay>();
        int numLog = BattleData.logDisplayList.Count;

        logDisplay.log = log;
        BattleData.logDisplayList.Add(logDisplay);
        logDisplayPanel.AddLogDisplay(logDisplay, numLog + 1);
    }
}