using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BattlePlayerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI playerLevelText;
    [SerializeField] private HPBar playerHpBar;
    [SerializeField] private SPBar playerSpBar;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI spText;

    [SerializeField] private TextMeshProUGUI maxHpText;
    [SerializeField] private TextMeshProUGUI maxSpText;
    [SerializeField] private GameObject selectedArrow;
    [SerializeField] private GameObject weakImage;
    [SerializeField] private GameObject invalidImage;

    [SerializeField] private Transform playerPos;
    [SerializeField] private Transform playerPersonaPos;

    private Player player;

    // プレイヤーの詳しいステータス画面
    [SerializeField] private GameObject playerDiscriptionPanel;

    [SerializeField] private Image[] statusSlots;
    [SerializeField] private Sprite[] statusSprites;
    [SerializeField] private TextMeshProUGUI[] statusTexts;

    public GameObject SelectedArrow { get => selectedArrow; }
    public Transform PlayerPersonaPos { get => playerPersonaPos; }
    public Transform PlayerPos { get => playerPos; set => playerPos = value; }

    public void SetPlayerData(Player player)
    {
        this.player = player;
        playerNameText.text = player.PlayerBase.PlayerName;
        playerLevelText.text = "Lv." + player.Level;
        playerHpBar.SetHP(player.Hp, player.MaxHp);
        playerSpBar.SetSP(player.Sp, player.MaxSp);
        hpText.text = player.Hp.ToString();
        spText.text = player.Sp.ToString();
        maxHpText.text = "/" + player.MaxHp.ToString();
        maxSpText.text = "/" + player.MaxSp.ToString();
        weakImage.SetActive(false);
        SetActivenessDiscriptionPanel(false);
    }

    public void UpdateHpSp()
    {
        playerHpBar.SetHP((float)player.Hp, player.MaxHp);
        playerSpBar.SetSP((float)player.Sp, player.MaxSp);
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

    // ステータス詳細画面をセットする関数
    public void SetStatusPanel(Player player)
    {
        // 初期化
        for (int i = 0; i < (int)World.MagicType.END - 1; i++)
        {
            statusSlots[i].sprite = statusSprites[2];
        }

        // 弱点表示設定
        // 弱点があれば
        World.MagicType[] weaknessTypes = player.PlayerBase.WeakTypes;
        for (int i = 0; i < weaknessTypes.Length; i++)
        {
            statusSlots[(int)weaknessTypes[i] - 1].sprite = statusSprites[0];
        }

        // 耐性表示設定
        World.MagicType[] resistanceTypes = player.PlayerBase.ResistanceTypes;
        for (int i = 0; i < resistanceTypes.Length; i++)
        {
            statusSlots[(int)resistanceTypes[i] - 1].sprite = statusSprites[1];
        }

        statusTexts[0].text = player.PlayerBase.PlayerMaxHp.ToString();
        statusTexts[1].text = player.PlayerBase.PlayerMaxSp.ToString();
        statusTexts[2].text = player.PlayerBase.PlayerPhysicAtk.ToString();
        statusTexts[3].text = player.PlayerBase.PlayerPhysicDef.ToString();
        //Debug.Log(player.PlayerBase.PlayerMagicAtk);
        statusTexts[4].text = player.PlayerBase.PlayerMagicAtk.ToString();
        statusTexts[5].text = player.PlayerBase.PlayerMagicDef.ToString();
    }

    public void SetActivenessDiscriptionPanel(bool activeness)
    {
        playerDiscriptionPanel.SetActive(activeness);
    }

    public void SetActiveSelectedArrow(bool activeness)
    {
        SelectedArrow.SetActive(activeness);
    }
}