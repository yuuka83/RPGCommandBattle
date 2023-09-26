using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerUnit : MonoBehaviour
{
    [SerializeField]
    private PlayerBase[] playerBasies;

    [SerializeField]
    private Transform[] playerBeforePos;

    [SerializeField]
    private GameObject[] playerPos;

    [SerializeField]
    private Player[] players;

    public Player[] Players { get => players; }

    public GameObject[] PlayerPos { get => playerPos; }
    public Transform[] PlayerBeforePos { get => playerBeforePos; }
    [SerializeField] private GameObject resultCanvas;

    public void SetUpFirst(int level)
    {
        int playerNum = 2;
        players = new Player[playerNum];
        Dictionary<Player, int> agiPlayerDic = new Dictionary<Player, int>();

        // 速さ順にInstantiate
        for (int i = 0; i < playerNum; i++)
        {
            List<Enemy> enemyList = this.gameObject.transform.Find("Player" + playerBasies[i].PlayerId + "Persona").GetComponent<PersonaParty>().EnemyList;
            // レベル１で生成
            Player player = new Player(playerBasies[i], level, enemyList);
            agiPlayerDic.Add(player, player.PlayerBase.PlayerAgi);
        }

        int j = 0;
        foreach (var player in agiPlayerDic.OrderByDescending(c => c.Value))
        {
            players[j] = player.Key;
            players[j].PlayerModel = Instantiate(players[j].PlayerBase.PlayerModel, PlayerBeforePos[j].transform);
            players[j].PlayerAnimator = players[j].PlayerModel.GetComponent<Animator>();
            players[j].PlayerUI = playerPos[j].transform.Find("PlayerCanvas/PlayerStatusPanel").gameObject.GetComponent<BattlePlayerUI>();
            players[j].PlayerUI.SetPlayerData(players[j]);
            Debug.Log("ResultPanel/player" + j + "ResultPanel");
            players[j].ResultPlayerUI = resultCanvas.transform.Find("ResultPanel/player" + j + "ResultPanel").GetComponent<ResultPlayerUI>();
            players[j].ResultPlayerUI.SetUpResultPanel(players[j]);
            j++;
        }
    }

    public void SetUp()
    {
        Dictionary<Player, int> agiPlayerDic = new Dictionary<Player, int>();
        // 速さ順にInstantiate
        for (int i = 0; i < Players.Length; i++)
        {
            List<Enemy> enemyList = this.gameObject.transform.Find("Player" + playerBasies[i].PlayerId + "Persona").GetComponent<PersonaParty>().EnemyList;
            // レベル１で生成
            agiPlayerDic.Add(Players[i], Players[i].PlayerBase.PlayerAgi);
        }
        int j = 0;
        foreach (var player in agiPlayerDic.OrderByDescending(c => c.Value))
        {
            players[j] = player.Key;
            players[j].PlayerModel = Instantiate(players[j].PlayerBase.PlayerModel, PlayerBeforePos[j].transform);
            players[j].PlayerAnimator = players[j].PlayerModel.GetComponent<Animator>();
            players[j].PlayerUI = playerPos[j].transform.Find("PlayerCanvas/PlayerStatusPanel").gameObject.GetComponent<BattlePlayerUI>();
            players[j].PlayerUI.SetPlayerData(players[j]);
            j++;
        }
    }
}