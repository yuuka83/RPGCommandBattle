using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ResultState
{
    PLAYER_ANIMATION,
    RESULT_LEVELUP,
    END
}

public class ResultSceneMangaer : MonoBehaviour
{
    private ResultState resultState = ResultState.PLAYER_ANIMATION;
    [SerializeField] private Transform winPlayerPos;
    [SerializeField] private Camera resultSceneCamera;
    [SerializeField] private GameObject resultPanel;
    private static readonly int hashWin = Animator.StringToHash("Base Layer.Win");
    [SerializeField] private GameManager gameManager;
    private GameObject playerModel;

    private void Update()
    {
        if (resultState == ResultState.END)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                gameManager.CurrentSceneIndex = (int)World.GameMode.FIELD_SCENE;
                Destroy(playerModel);
                resultPanel.SetActive(false);
            }
        }
    }

    // とどめを刺したプレイヤーの生成
    // カメラの移動
    public IEnumerator ResultPlayer(Player player)
    {
        Debug.Log(player.PlayerBase.PlayerName);
        playerModel = Instantiate(player.PlayerBase.PlayerModel, winPlayerPos.position, Quaternion.identity);
        // アニメーション
        player.PlayerAnimator = playerModel.GetComponent<Animator>();
        player.PlayerAnimator.Play(hashWin);
        yield return null;
        yield return new WaitForAnimation(player.PlayerAnimator, 0);
        // リザルトシーンの表示
        resultPanel.SetActive(true);
        resultState = ResultState.RESULT_LEVELUP;
        IEnumerator enumerator = gameManager.UpdateExpAnimation();
        yield return enumerator;
        resultState = ResultState.END;
    }
}