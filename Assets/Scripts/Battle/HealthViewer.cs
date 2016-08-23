using UnityEngine;
using System.Collections;
using Enums;

public class HealthViewer : MonoBehaviour {

	GameObject currentHealthBar;
	GameObject recoverBar;
	GameObject damageBar;

	int currentHealth;
	int maxHealth;

	public void PreviewDamageAmount(int damageAmount)
	{
		Debug.Log("PreviewDamageAmount");
		int previewCurrentHealth = currentHealth - damageAmount;
		float healthRatio = (float)previewCurrentHealth / (float)maxHealth;
		Vector3 previewCurrentHealthScale = new Vector3(healthRatio, 1, 1);
		currentHealthBar.transform.localScale = previewCurrentHealthScale;
		recoverBar.transform.localScale = previewCurrentHealthScale;
	}
	
	public void PreviewRecoverAmount(int recoverAmount)
	{
		int previewCurrentHealth = currentHealth + recoverAmount;
		float healthRatio = (float)previewCurrentHealth / (float)maxHealth;
		Vector3 previewCurrentHealthScale = new Vector3(healthRatio, 1, 1);
		recoverBar.transform.localScale = previewCurrentHealthScale;
	}
	
	public void CancelPreview()
	{
		Vector3 damageBarScale = damageBar.transform.localScale;
		currentHealthBar.transform.localScale = damageBarScale;
		recoverBar.transform.localScale = damageBarScale; 
	}

	public void UpdateCurrentHealth(int currentHealth, int maxHealth)
	{
		float healthRatio = (float)currentHealth / (float)maxHealth;
		Vector3 currentHealthScale = new Vector3(healthRatio, 1, 1);
		currentHealthBar.transform.localScale = currentHealthScale;
		recoverBar.transform.localScale = currentHealthScale;
		damageBar.transform.localScale = currentHealthScale;
		
		this.currentHealth = currentHealth;
		this.maxHealth = maxHealth;
	}
	
	public void SetInitHealth(int maxHealth, Side side)
	{
		this.currentHealth = maxHealth;
		this.maxHealth = maxHealth;
		
		Vector3 initHealthScale = new Vector3(1, 1, 1);
		currentHealthBar.transform.localScale = initHealthScale;
		recoverBar.transform.localScale = initHealthScale;
		damageBar.transform.localScale = initHealthScale;
		
		if (side == Side.Ally)
		{
			currentHealthBar.GetComponent<SpriteRenderer>().color = Color.cyan;	
		}
		else
		{
			currentHealthBar.GetComponent<SpriteRenderer>().color = Color.yellow;
		}
	}

	void Awake () {
		currentHealthBar = transform.Find("currentHealthBar").gameObject;
		recoverBar = transform.Find("recoverBar").gameObject;
		damageBar = transform.Find("damageBar").gameObject;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
