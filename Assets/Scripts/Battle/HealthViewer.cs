using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;
using GameData;
using Battle.Turn;

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

    public void Preview(float health, float shield) {
        List<GameObject> barsToBeHealthScale = new List<GameObject> { recoverBar };
        List<GameObject> barsToBeShieldScale = new List<GameObject> { shieldRecoverBar };
        if (health < this.currentHealth) {
            barsToBeHealthScale.Add(currentHealthBar);
            barsToBeShieldScale.Add(shieldBar);
        }
        else barsToBeShieldScale.Add(shieldBar);
        if(shield  <= this.shieldAmount )  barsToBeShieldScale.Add(shieldBar);
        else {
            barsToBeHealthScale.Add(currentHealthBar);
            barsToBeHealthScale.Add(damageBar);
        }
        AdjustBarScales(health, shield, barsToBeHealthScale, barsToBeShieldScale);
        
        //이하는 처치/이탈 아이콘 표시
        Debug.Log("Called");
        var type = GetComponentInParent<Unit>().GetDestroyReason((int)health);
        Debug.Log(type+"");
        if(type == TrigActionType.Kill) {killIcon.SetActive(true);}
        else if(type == TrigActionType.Retreat) {retreatIcon.SetActive(true);}
    }

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

	public void SetInitHealth(int maxHealth, Side side, bool isAI, bool isNamed){
		this.currentHealth = maxHealth;
		this.maxHealth = maxHealth;
		
		Vector3 initHealthScale = new Vector3(1, 1, 1);
		currentHealthBar.transform.localScale = initHealthScale;
		recoverBar.transform.localScale = initHealthScale;
		damageBar.transform.localScale = initHealthScale;
        shieldBar.transform.localScale = initHealthScale;
        shieldDamageBar.transform.localScale = initHealthScale;
		currentHealthBar.GetComponent<SpriteRenderer>().color = SideToHealthColor(side, isAI, isNamed);
	}

	public static Color SideToHealthColor(Side side, bool isAI, bool isNamed){
		if (side == Side.Ally) {
			if (isAI) {
				return new Color (100f/255f, 160f/255f, 1);
			} else {
				return new Color (45f/255f, 80f/255f, 200f/255f);
			}
		} else if (side == Side.Neutral) {
			return Color.gray;
		} else {
			if (isNamed) {
				return new Color (255f / 255f, 130f / 255f, 0f / 255f); // boss orange(for test)
			} else {
				return new Color (1, 200f / 255f, 50f / 255f);
			}
		}
    }

	void Awake () {
		currentHealthBar = transform.Find("currentHealthBar").gameObject;
        shieldBar = transform.Find("ShieldBar").gameObject;
		shieldBar.GetComponent<SpriteRenderer> ().color = new Color (0.8f, 0.8f, 0.8f);
		recoverBar = transform.Find("recoverBar").gameObject;
		recoverBar.GetComponent<SpriteRenderer> ().color = new Color (0f, 1.0f, 0.25f);
		damageBar = transform.Find("damageBar").gameObject;
		damageBar.GetComponent<SpriteRenderer> ().color = new Color (0.7f, 0f, 0f);
		shieldDamageBar = transform.Find("ShieldDamageBar").gameObject;
		shieldDamageBar.GetComponent<SpriteRenderer> ().color = new Color (0.9f, 0.3f, 0.3f);
		shieldRecoverBar = transform.Find("ShieldRecoverBar").gameObject;
		shieldRecoverBar.GetComponent<SpriteRenderer> ().color = new Color (0.2f, 0.8f, 0.45f);
		retreatIcon = transform.Find("Retreat").gameObject;
		killIcon = transform.Find("Kill").gameObject;
	}
}