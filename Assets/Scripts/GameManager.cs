using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject[] fieldObjects;

    public static int currentSceneIndex;
    private World.GameMode currentGameMode;
    [SerializeField] private BattleSceneManager battleSceneManager;
    [SerializeField] private ResultSceneMangaer resultSceneManager;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerUnit playerUnit;
    [SerializeField] private EnemyUnit enemyUnit;
    private List<Player> players;

    [SerializeField]
    private List<Enemy> enemies;

    [SerializeField] private ExpSheet expSheet;// 経験値表

    private void Update()
    {
        if (currentSceneIndex == (int)World.GameMode.FIELD_SCENE)
        {
            playerController.HandleUpdate();
        }
        else if (currentSceneIndex == (int)World.GameMode.BATTLE_SCENE)
        {
            battleSceneManager.HandleUpdate();
        }
        else if (currentSceneIndex == (int)World.GameMode.RESULT_SCENE)
        {
        }
    }

    public int CurrentSceneIndex
    {
        get { return currentSceneIndex; }
        set
        {
            currentSceneIndex = value;
            ActivateCurrentScene(currentSceneIndex);
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        currentSceneIndex = (int)World.GameMode.FIELD_SCENE;
    }

    private void ActivateCurrentScene(int sceneIndex)
    {
        currentSceneIndex = sceneIndex;
        for (int i = 0; i < fieldObjects.Length; i++)
        {
            fieldObjects[i].SetActive(false);
        }
        fieldObjects[sceneIndex].SetActive(true);
    }

    private bool FirstBattle = true;

    public void StartBattle()
    {
        ActivateCurrentScene((int)World.GameMode.BATTLE_SCENE);
        battleSceneManager.StartBattle();
        // 生成
        players = new List<Player>();
        enemies = new List<Enemy>();
        if (FirstBattle == true)
        {
            playerUnit.SetUpFirst(1);
            FirstBattle = false;
        }
        else
        {
            playerUnit.SetUp();
        }
        // モンスターの生成
        enemyUnit.SetUp(1, 3);
        for (int i = 0; i < playerUnit.Players.Length; i++)
        {
            players.Add(playerUnit.Players[i]);
        }
        for (int i = 0; i < enemyUnit.Enemies.Length; i++)
        {
            enemies.Add(enemyUnit.Enemies[i]);
        }
        Debug.Log("enemyCount" + enemies.Count);

        battleSceneManager.InitBattle(players, enemies);
    }

    public void EndBattle()
    {
        ActivateCurrentScene((int)World.GameMode.RESULT_SCENE);
        StartCoroutine(resultSceneManager.ResultPlayer((Player)battleSceneManager.TurnCharacter));
    }

    public IEnumerator UpdateExpAnimation()
    {
        // 経験値の処理
        int exp = 0;
        Debug.Log("enemyCount" + enemies.Count);
        for (int i = 0; i < enemies.Count; i++)
        {
            exp += expSheet.sheets[0].list[enemies[i].Level - 1].exp;
        }
        // floatでよみこまなきゃだめ？
        Debug.Log("getExp" + exp);
        IEnumerator enumerator = null;
        for (int i = 0; i < players.Count; i++)
        {
            Debug.Log(players[i].PlayerBase.PlayerName);
            ExpPair expPair = players[i].GetExp(exp);
            players[i].ResultPlayerUI.SetPlayerNameText();
            enumerator = players[i].ResultPlayerUI.UpdateExp(expPair);
            StartCoroutine(enumerator);
            //players[i].ResultPlayerUI.SetPlayerLevelText();
        }
        yield return enumerator;
        //ActivateCurrentScene((int)World.GameMode.FIELD_SCENE);
    }
}