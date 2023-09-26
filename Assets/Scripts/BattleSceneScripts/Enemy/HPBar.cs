using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
	// HPバーの描画
	[SerializeField]
	Image hpBar;

	public void SetHP(float hp,float maxHp)
	{
		hpBar.GetComponent<Image>().fillAmount = hp / maxHp;
	}
	/*
	public IEnumerator SetHPSmooth(float newHp,float maxHp)
	{
		float currentHp = hpBar.GetComponent<Image>().fillAmount;//1
		newHp = newHp / maxHp;// 11/13
		Debug.Log("現在のHP"+currentHp*maxHp);
		Debug.Log("新しいHP"+newHp*maxHp);
		float changeAmount = currentHp - newHp;// 減少量0~1の間 2/13
		Debug.Log("変化量"+ changeAmount);
		// curretHpとnewHpに差があるなら繰り返す
		while (currentHp - newHp > Mathf.Epsilon)// 2/13
		{
			// 1秒でchangeAmount分減る
			currentHp -= changeAmount * Time.deltaTime;
			hpBar.GetComponent<Image>().fillAmount = currentHp;
			yield return null;

		}
		hpBar.GetComponent<Image>().fillAmount = newHp;
	
	}*/
}
