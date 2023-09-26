using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class BattleCommand : MonoBehaviour
{
	[SerializeField] GameObject battleCommandPanel;
	[SerializeField] GameObject skillCommandPanel;
	//[SerializeField] GameObject itemCommandPanel;
	[SerializeField] TextMeshProUGUI dialogText;
	[SerializeField] GameObject[] battleCommandSelectArrow;
	[SerializeField] GameObject[] skillSelectArrow;

	// テキストを変更するための関数
	public void SetDialog(string dialog)
	{
		dialogText.text = dialog;
	}

	// 1秒表示する
	public void EnableDialogText(bool enabled)
	{
		dialogText.enabled = enabled;
	}

	// メインパネルをオンオフする関数
	public void ActivateBattleCommandPanel(bool activate)
	{
		battleCommandPanel.SetActive(activate);
	}
	public void ActivateBattleSelectArrow(Move move)
	{
		for(int i = 0;i < battleCommandSelectArrow.Length; i++)
		{
			battleCommandSelectArrow[i].SetActive(false);
		}
		battleCommandSelectArrow[(int)move].SetActive(true);
	}
	// スキルパネルをオンオフする関数
	public void ActivateSkillCommandPanel(bool activate)
	{
		skillCommandPanel.SetActive(activate);
	}
	// スキルパネルの矢印
	public void ActivateSkillSelectArrow(int skillNum)
	{
		for (int i = 0; i < skillSelectArrow.Length; i++)
		{
			skillSelectArrow[i].SetActive(false);
		}
		skillSelectArrow[skillNum].SetActive(true);
	}
}
