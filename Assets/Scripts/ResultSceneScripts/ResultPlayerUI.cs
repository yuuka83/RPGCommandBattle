using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ResultPlayerUI : MonoBehaviour
{
    [SerializeField] private ExpBar playerExpBar;
    [SerializeField] private TextMeshProUGUI playerNameText;
    private Player player;

    public void SetUpResultPanel(Player player)
    {
        this.player = player;
    }

    public IEnumerator UpdateExp(ExpPair expPair)
    {
        Debug.Log("updateExp");
        IEnumerator enumerator = playerExpBar.SetExpSmooth(expPair);
        yield return enumerator;
    }

    public void SetPlayerNameText()
    {
        playerNameText.text = player.PlayerBase.PlayerName;
    }
}