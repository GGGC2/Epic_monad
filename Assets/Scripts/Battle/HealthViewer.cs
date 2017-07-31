using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enums;

public class HealthViewer : MonoBehaviour {
    /*height order : 
     * currentHealthBar         ( z = -0.03 )
     * recoverBar, damageBar    ( z = -0.02 )
     * shieldBar                ( z = -0.01 )
     * shieldDamageBar          ( z =  0    )
     * backgroundBar            ( z =  0.01 )
     */
	GameObject currentHealthBar;
    GameObject shieldBar;
	GameObject recoverBar;
	GameObject damageBar;
    GameObject shieldDamageBar;
	GameObject retreatIcon;
	GameObject killIcon;

	int currentHealth;
	int maxHealth;
    int shieldAmount;

	public void PreviewDamageAmount(int damageAmount) {
        int previewCurrentHealth = currentHealth + Math.Min(shieldAmount - damageAmount, 0);
        previewCurrentHealth = Math.Max(previewCurrentHealth, 0);
        int previewShieldAmount = Math.Max(shieldAmount - damageAmount, 0);
        GameObject[] barsToBeHealthScale = { currentHealthBar, recoverBar };
        GameObject[] barsToBeShieldScale = { shieldBar };
        float previewHealthRatio = AdjustBarScales(previewCurrentHealth, previewShieldAmount, barsToBeHealthScale, barsToBeShieldScale);

        if (previewHealthRatio <= 0)   killIcon.SetActive(true);
		else if ((previewHealthRatio <= 0.1) && 
                !(transform.parent.gameObject.GetComponent<Unit>().IsObject())) 
                retreatIcon.SetActive(true); // 오브젝트는 이탈하지 않음 (=오브젝트가 아닐 경우에만 이탈).
	}
	
	public void PreviewRecoverAmount(int recoverAmount) {
		int previewCurrentHealth = Math.Min(currentHealth + recoverAmount, maxHealth);
        GameObject[] barsToBeHealthScale = { recoverBar };
        GameObject[] barsToBeShieldScale = { shieldBar };
        AdjustBarScales(previewCurrentHealth, shieldAmount, barsToBeHealthScale, barsToBeShieldScale);
	}

	public void UpdateCurrentHealth(int currentHealth, int shieldAmount, int maxHealth) {
        this.currentHealth = currentHealth;
        this.maxHealth = maxHealth;
        this.shieldAmount = shieldAmount;
        GameObject[] barsToBeHealthScale = { currentHealthBar, recoverBar, damageBar };
        GameObject[] barsToBeShieldScale = { shieldBar, shieldDamageBar };
        AdjustBarScales(currentHealth, shieldAmount, barsToBeHealthScale, barsToBeShieldScale );
	}

    private float AdjustBarScales(float health, float shieldAmount, GameObject[] barsToBeHealthScale, GameObject[] barsToBeShieldScale) {
        //Adjusts barScales according to parameters, and returns healthRatio
        float maxBarSize = Math.Max(this.currentHealth + this.shieldAmount, maxHealth);
        float healthRatio = health / maxBarSize;
        float shieldRatio = shieldAmount / maxBarSize;

        Vector3 healthScale = new Vector3(healthRatio, 1, 1);
        Vector3 shieldScale = new Vector3(healthRatio + shieldRatio, 1, 1);

        foreach(var bar in barsToBeHealthScale)     bar.transform.localScale = healthScale;
        foreach(var bar in barsToBeShieldScale)     bar.transform.localScale = shieldScale;
        return healthScale.x;
    }

    public void CancelPreview() {
        Vector3 damageBarScale = damageBar.transform.localScale;
        currentHealthBar.transform.localScale = damageBarScale;
        recoverBar.transform.localScale = damageBarScale;

        Vector3 shieldDamageBarScale = shieldDamageBar.transform.localScale;
        shieldBar.transform.localScale = shieldDamageBarScale;
        killIcon.SetActive(false);
        retreatIcon.SetActive(false);
    }

    public void SetInitHealth(int maxHealth, Side side)
	{
		this.currentHealth = maxHealth;
		this.maxHealth = maxHealth;
		
		Vector3 initHealthScale = new Vector3(1, 1, 1);
		currentHealthBar.transform.localScale = initHealthScale;
		recoverBar.transform.localScale = initHealthScale;
		damageBar.transform.localScale = initHealthScale;
        shieldBar.transform.localScale = initHealthScale;
        shieldDamageBar.transform.localScale = initHealthScale;
		
		if (side == Side.Ally)
		{
			currentHealthBar.GetComponent<SpriteRenderer>().color = Color.cyan;	
		}
		else if (side == Side.Neutral)
		{
			currentHealthBar.GetComponent<SpriteRenderer>().color = Color.gray;
		}
		else
		{
			currentHealthBar.GetComponent<SpriteRenderer>().color = Color.yellow;
		}
	}

	void Awake () {
		currentHealthBar = transform.Find("currentHealthBar").gameObject;
        shieldBar = transform.Find("ShieldBar").gameObject;
        recoverBar = transform.Find("recoverBar").gameObject;
        damageBar = transform.Find("damageBar").gameObject;
        shieldDamageBar = transform.Find("ShieldDamageBar").gameObject;
		retreatIcon = transform.Find("Retreat").gameObject;
		killIcon = transform.Find("Kill").gameObject;
	}
}