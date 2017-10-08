﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;
using GameData;

public class HealthViewer : MonoBehaviour {
    /*height order : 
     * currentHealthBar                     ( z = -0.03 )
     * recoverBar, damageBar                ( z = -0.02 )
     * shieldBar                            ( z = -0.01 )
     * shieldRecoverBar, shieldDamageBar    ( z =  0    )
     * backgroundBar                        ( z =  0.01 )
     */
	GameObject currentHealthBar;
	GameObject recoverBar;
	GameObject damageBar;
    GameObject shieldBar;
    GameObject shieldRecoverBar;
    GameObject shieldDamageBar;
	GameObject retreatIcon;
	GameObject killIcon;

	int currentHealth;
	int maxHealth;
    int shieldAmount;

    public void PreviewDamageInfoList(List<Battle.DamageCalculator.DamageInfo> damageInfoList) {
        int currentHealth = this.currentHealth;
        int shieldAmount  = this.shieldAmount;
        foreach(var damageInfo in damageInfoList) {
            int damageAmount = (int)damageInfo.damageAmount;
            if (damageAmount > 0) {
                currentHealth += Math.Min(shieldAmount - damageAmount, 0);
                currentHealth = Math.Max(currentHealth, 0);
                shieldAmount = Math.Max(shieldAmount - damageAmount, 0);
            } else currentHealth = Math.Min(currentHealth - damageAmount, maxHealth);
            if(damageInfo.shieldAmount > 0)    shieldAmount += (int)damageInfo.shieldAmount;
        }
        List<GameObject> barsToBeHealthScale = new List<GameObject> { recoverBar };
        List<GameObject> barsToBeShieldScale = new List<GameObject> { shieldRecoverBar };
        if (currentHealth < this.currentHealth) {
            barsToBeHealthScale.Add(currentHealthBar);
            barsToBeShieldScale.Add(shieldBar);
        }
        else barsToBeShieldScale.Add(shieldBar);
        if(shieldAmount  <= this.shieldAmount )  barsToBeShieldScale.Add(shieldBar);
        else {
            barsToBeHealthScale.Add(currentHealthBar);
            barsToBeHealthScale.Add(damageBar);
        }
        AdjustBarScales(currentHealth, shieldAmount, barsToBeHealthScale, barsToBeShieldScale);
    }

	/*public void PreviewDamageAmount(int damageAmount) {
        int previewCurrentHealth = currentHealth + Math.Min(shieldAmount - damageAmount, 0);
        previewCurrentHealth = Math.Max(previewCurrentHealth, 0);
        int previewShieldAmount = Math.Max(shieldAmount - damageAmount, 0);
        GameObject[] barsToBeHealthScale = { currentHealthBar, recoverBar };
        GameObject[] barsToBeShieldScale = { shieldBar, shieldRecoverBar };
        float previewHealthRatio = AdjustBarScales(previewCurrentHealth, previewShieldAmount, barsToBeHealthScale, barsToBeShieldScale);

        if (previewHealthRatio <= 0)   killIcon.SetActive(true);
		else if ((previewHealthRatio <= Setting.retreatHpFloat) && 
                !(transform.parent.gameObject.GetComponent<Unit>().IsObject) && SceneData.stageNumber >= Setting.retreatOpenStage)
                retreatIcon.SetActive(true); // 오브젝트는 이탈하지 않음 (=오브젝트가 아닐 경우에만 이탈).
	}
	
	public void PreviewRecoverAmount(int recoverAmount) {
		int previewCurrentHealth = Math.Min(currentHealth + recoverAmount, maxHealth);
        GameObject[] barsToBeHealthScale = { recoverBar };
        GameObject[] barsToBeShieldScale = { shieldBar , shieldRecoverBar};
        AdjustBarScales(previewCurrentHealth, shieldAmount, barsToBeHealthScale, barsToBeShieldScale);
	}

    public void PreviewShieldAmount(int shieldAmount) {
        int previewShieldAmount = shieldAmount + this.shieldAmount;
        GameObject[] barsToBeHealthScale = { currentHealthBar, recoverBar, damageBar};
        GameObject[] barsToBeShieldScale = { shieldRecoverBar };
        AdjustBarScales(currentHealth, previewShieldAmount, barsToBeHealthScale, barsToBeShieldScale);
    }*/

	public void UpdateCurrentHealth(int currentHealth, int shieldAmount, int maxHealth) {
        this.currentHealth = currentHealth;
        this.maxHealth = maxHealth;
        this.shieldAmount = shieldAmount;
        List<GameObject> barsToBeHealthScale = new List<GameObject> { currentHealthBar, recoverBar, damageBar };
        List<GameObject> barsToBeShieldScale = new List<GameObject> { shieldBar, shieldRecoverBar, shieldDamageBar };
        AdjustBarScales(currentHealth, shieldAmount, barsToBeHealthScale, barsToBeShieldScale );
	}

    private float AdjustBarScales(float health, float shieldAmount, List<GameObject> barsToBeHealthScale, List<GameObject> barsToBeShieldScale) {
        //Adjusts barScales according to parameters, and returns healthRatio
        float maxBarSize = Math.Max(this.currentHealth + Math.Max(shieldAmount, this.shieldAmount), maxHealth);
        float healthRatio = health / maxBarSize;
        float shieldRatio = shieldAmount / maxBarSize;

        Vector3 healthScale = new Vector3(healthRatio, 1, 1);
        Vector3 shieldScale = new Vector3(healthRatio + shieldRatio, 1, 1);

        foreach(var bar in barsToBeShieldScale)     bar.transform.localScale = shieldScale;
        foreach (var bar in barsToBeHealthScale) bar.transform.localScale = healthScale;
        return healthScale.x;
    }

    public void CancelPreview() {
        UpdateCurrentHealth(currentHealth, shieldAmount, maxHealth);
        killIcon.SetActive(false);
        retreatIcon.SetActive(false);
    }

    public void SetInitHealth(int maxHealth, Side side){
		this.currentHealth = maxHealth;
		this.maxHealth = maxHealth;
		
		Vector3 initHealthScale = new Vector3(1, 1, 1);
		currentHealthBar.transform.localScale = initHealthScale;
		recoverBar.transform.localScale = initHealthScale;
		damageBar.transform.localScale = initHealthScale;
        shieldBar.transform.localScale = initHealthScale;
        shieldDamageBar.transform.localScale = initHealthScale;
		currentHealthBar.GetComponent<SpriteRenderer>().color = SideToHealthColor(side);
	}

    public static Color SideToHealthColor(Side side){
        if (side == Side.Ally) {return Color.cyan;}
		else if (side == Side.Neutral) {return Color.gray;}
		else {return Color.yellow;}
    }

	void Awake () {
		currentHealthBar = transform.Find("currentHealthBar").gameObject;
        shieldBar = transform.Find("ShieldBar").gameObject;
        recoverBar = transform.Find("recoverBar").gameObject;
        damageBar = transform.Find("damageBar").gameObject;
        shieldDamageBar = transform.Find("ShieldDamageBar").gameObject;
        shieldRecoverBar = transform.Find("ShieldRecoverBar").gameObject;
		retreatIcon = transform.Find("Retreat").gameObject;
		killIcon = transform.Find("Kill").gameObject;
	}
}