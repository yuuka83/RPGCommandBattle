using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleEnemyUI : MonoBehaviour
{
    // 敵
    [SerializeField] private TextMeshProUGUI enemyNameText;

    [SerializeField] private TextMeshProUGUI enemyLevelText;
    [SerializeField] private HPBar enemyHpBar;
    [SerializeField] private GameObject weakImage;
    [SerializeField] private GameObject invalidImage;

    private Enemy enemy;

    // 敵の詳細
    [SerializeField] private GameObject selectedArrow;

    [SerializeField] private GameObject EnemyDiscriptionPanel;
    [SerializeField] private Image[] statusSlots;
    [SerializeField] private Sprite[] statusSprites;

    public GameObject SelectedArrow { get => selectedArrow; set => selectedArrow = value; }

    public void SetEnemyData(Enemy enemy)
    {
        this.enemy = enemy;
        enemyNameText.text = enemy.EnemyBase.EnemyName;
        enemyLevelText.text = "Lv." + enemy.Level.ToString();
        enemyHpBar.SetHP(enemy.Hp, enemy.MaxHp);
        //weakImage.SetActive(false);
        // 弱点表示設定
        World.MagicType[] weaknessType = enemy.EnemyBase.WeakTypes;
        if (weaknessType[0] != World.MagicType.NOTHING)
        {
            for (int i = 0; i < weaknessType.Length; i++)
            {
                statusSlots[(int)weaknessType[i] - 1].sprite = statusSprites[0];
            }
        }
        // 耐性表示設定
        World.MagicType[] resistanceType = enemy.EnemyBase.ResistanceTypes;
        if (resistanceType[0] != World.MagicType.NOTHING)
        {
            for (int i = 0; i < resistanceType.Length; i++)
            {
                statusSlots[(int)resistanceType[i] - 1].sprite = statusSprites[1];
            }
        }
        // 無効表示

        World.MagicType[] invalidType = enemy.EnemyBase.ResistanceTypes;
        if (resistanceType[0] != World.MagicType.NOTHING)
        {
            for (int i = 0; i < invalidType.Length; i++)
            {
                statusSlots[(int)invalidType[i] - 1].sprite = statusSprites[1];
            }
        }

        SetActivenessDiscriptionPanel(false);
    }

    public void UpdateHp()
    {
        Debug.Log("enemyName" + enemy.EnemyBase.EnemyName + "hp" + enemy.Hp + "maxhp" + enemy.MaxHp);
        // コルーチンの中でStartCoroutineは省略可能
        enemyHpBar.SetHP((float)enemy.Hp, enemy.MaxHp);
        //enemyHpBar.SetHP((float)enemy.Hp,enemy.MaxHp);
    }

    public void SetActiveWeakInfo()
    {
        StartCoroutine("SetActiveWeakInfoSeconds");
    }

    public void SetActiveInvalidInfo()
    {
        StartCoroutine("SetActiveInvalidInfoSeconds");
    }

    private IEnumerator SetActiveWeakInfoSeconds()
    {
        weakImage.SetActive(true);
        yield return new WaitForSeconds(1);
        weakImage.SetActive(false);
    }

    private IEnumerator SetActiveInvalidInfoSeconds()
    {
        invalidImage.SetActive(true);
        yield return new WaitForSeconds(1);
        invalidImage.SetActive(false);
    }

    public void SetActivenessDiscriptionPanel(bool activeness)
    {
        EnemyDiscriptionPanel.SetActive(activeness);
    }

    public void SetActiveSelectedArrow(bool activeness)
    {
        SelectedArrow.SetActive(activeness);
    }

    public void UnActiveUIPanel()
    {
        this.GetComponent<Canvas>().enabled = false;
    }

    public void ActivateUIPanel()
    {
        this.GetComponent<Canvas>().enabled = true;
    }
}