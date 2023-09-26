using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SPBar : MonoBehaviour
{
	// SPバーの描画
	[SerializeField]
	Image spBar;

	public void SetSP(float sp, float maxSp)
	{
		spBar.GetComponent<Image>().fillAmount = sp / maxSp;
	}
	/*
	public IEnumerator SetSPSmooth(float newSp, float maxSp)
	{
		float currentSp = spBar.GetComponent<Image>().fillAmount;
		float changeAmount = currentSp - newSp / maxSp;
		// curretHpとnewHpに差があるなら繰り返す
		while (currentSp - newSp > Mathf.Epsilon)
		{
			// 1秒でchangeAmount分減る
			currentSp -= changeAmount * Time.deltaTime;
			Debug.Log("currentHp" + currentSp);
			yield return null;
			spBar.GetComponent<Image>().fillAmount = currentSp;

		}
		spBar.GetComponent<Image>().fillAmount = newSp / maxSp;

	}*/
}
