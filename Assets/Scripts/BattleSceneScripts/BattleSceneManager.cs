using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using EnhancedUI.EnhancedScroller;

public enum BattleState
{
    INIT_BATTLE,
    PREPARE_BATTLE,
    PLAYER_ACTION_SELECT,
    PLAYER_MOVE,
    ENEMY_MOVE,
    BUSY,
    END_BATTLE,
}

public enum Move
{
    ATTACK,
    ITEM,
    ESCAPE,
    END
}

// スキルについての入力の状態遷移を表す列挙型「InputSkillStatement」を定義する
internal enum InputSkillStatement
{
    INIT_SKILL,
    SKILL_SELECT,
    TARGET_SELECT,
    END_INPUT
}

// どこで気絶状態を戻すか問題
// 前のターンに気絶していたかどうか
public class BattleSceneManager : MonoBehaviour, IEnhancedScrollerDelegate
{
    [SerializeField] private GameManager gameManager;

    // [SerializeField] private PlayerUnit playerUnit;
    //[SerializeField] private EnemyUnit enemyUnit;

    [SerializeField] private List<Enemy> activeEnemies;

    //[SerializeField] List<GameObject> activeEnemyModels;
    [SerializeField] private List<Player> activePlayers;

    [SerializeField] private float speed;

    //[SerializeField] private List<BattleEnemyUI> battleEnemyUIs;
    [SerializeField] private DialogBox dialogBox;

    [SerializeField] private BattleCommand battleCommand;

    private BattleState battleState = BattleState.BUSY;
    private InputSkillStatement inputSkillStatement;

    [SerializeField]
    private List<Character> characters;// 戦闘に参加してるキャラクター

    private int turnCharacterIndex;
    [SerializeField] private Character turnCharacter;// 行動順のキャラクター

    [SerializeField]
    private Enemy[] selectedEnemies;

    [SerializeField]
    private Player[] selectedPlayers;

    private bool hitWeakFirst;// 弱点をついたかどうか

    /// スキル関係==================================
    private List<SkillCellData> skillDatas;

    private int selectedSkillIndex;// リストの選択されたインデックス

    // デリゲートのためのスクローラー　
    // スクローラーに定義されたタイミングで実行する関数を書く
    public EnhancedScroller playerSkillPanel;

    // スクローラーの各セルのプレハブ
    public EnhancedScrollerCellView cellViewPrefab;

    public float cellSize;

    private int selectedTargetIndex;

    //アニメーション関係=============================================
    private static readonly int hashDamage = Animator.StringToHash("Base Layer.Damage");

    private static readonly int hashFaint = Animator.StringToHash("Base Layer.Faint");
    private static readonly int hashHeal = Animator.StringToHash("Base Layer.Heal");
    private static readonly int hashInvalid = Animator.StringToHash("Base Layer.Invalid");
    private static readonly int hashAttack = Animator.StringToHash("Base Layer.Attack");
    private static readonly int hashSkill = Animator.StringToHash("Base Layer.Skill");

    // 現在のMove
    private int currentMove;

    public Character TurnCharacter { get => turnCharacter; set => turnCharacter = value; }

    public void StartBattle()
    {
        Application.targetFrameRate = 60;
        battleState = BattleState.INIT_BATTLE;
    }

    public void HandleUpdate()
    {
        if (battleState == BattleState.INIT_BATTLE)
        {
            //InitBattle();
        }
        else if (battleState == BattleState.PREPARE_BATTLE)
        {
            // プレイヤーが走ってくる
            PrepareBattle();
        }
        else if (battleState == BattleState.PLAYER_ACTION_SELECT)
        {
            HandleActionSelection();
        }
        else if (battleState == BattleState.PLAYER_MOVE)
        {
            if (inputSkillStatement == InputSkillStatement.INIT_SKILL)
            {
                PlayerAction();
            }
            else if (inputSkillStatement == InputSkillStatement.SKILL_SELECT)
            {
                HandleSkillSelection();
            }
            else if (inputSkillStatement == InputSkillStatement.TARGET_SELECT)
            {
                Player turnPlayer = (Player)turnCharacter;
                HandleSelectTarget(turnPlayer.Skills[selectedSkillIndex]);
            }
            else if (inputSkillStatement == InputSkillStatement.END_INPUT)
            {
                PlayerEndSkillInput();
            }
        }
        else if (battleState == BattleState.ENEMY_MOVE)
        {
            if (inputSkillStatement == InputSkillStatement.INIT_SKILL)
            {
                EnemyMove();
            }
            else if (inputSkillStatement == InputSkillStatement.SKILL_SELECT)
            {
                EnemySkillSelect();
            }
            else if (inputSkillStatement == InputSkillStatement.TARGET_SELECT)
            {
                EnemyTargetSelect(activeEnemies[0].Skills[selectedSkillIndex]);
            }
            else if (inputSkillStatement == InputSkillStatement.END_INPUT)
            {
                EnemyEndInput();
            }
        }
    }

    // 戦闘が始まる前に一回だけ実行する===========================
    public void InitBattle(List<Player> players, List<Enemy> enemies)
    {
        inputSkillStatement = InputSkillStatement.INIT_SKILL;
        // プレイヤーの生成
        /*
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
        */
        characters = new List<Character>();
        activePlayers = new List<Player>();
        activeEnemies = new List<Enemy>();
        //activePlayers = players;
        //activeEnemies = enemies;
        // リストは参照渡しになる
        for (int i = 0; i < players.Count; i++)
        {
            activePlayers.Add(players[i]);
        }
        for (int i = 0; i < enemies.Count; i++)
        {
            activeEnemies.Add(enemies[i]);
        }

        // キャラクターが持ち場につく
        battleState = BattleState.PREPARE_BATTLE;

        Dictionary<Character, int> agiCharaDictionary = new Dictionary<Character, int>();

        for (int i = 0; i < activePlayers.Count; i++)
        {
            agiCharaDictionary.Add(activePlayers[i], activePlayers[i].PlayerBase.PlayerAgi);
        }
        for (int i = 0; i < activeEnemies.Count; i++)
        {
            agiCharaDictionary.Add(activeEnemies[i], activeEnemies[i].EnemyBase.Agi);
        }

        // 初期化
        characters = new List<Character>();

        // Enemyのソート
        // ソート
        foreach (var Chara in agiCharaDictionary.OrderByDescending(c => c.Value))
        {
            characters.Add(Chara.Key);
            Debug.Log(Chara.Value);
            if (Chara.Key.isPlayer)
            {
                // デバッグ用
                Player player = (Player)Chara.Key;
                Debug.Log("player:" + player.PlayerBase.PlayerName);
            }
            else
            {
                Enemy enemy = (Enemy)Chara.Key;
                Debug.Log("enemy:" + enemy.EnemyBase.EnemyName);
            }
        }

        // charactersに代入
        for (int i = 0; i < characters.Count; i++)
        {
            if (characters[i].isPlayer)
            {
                Player player = (Player)characters[i];
                // Debug.Log(player.PlayerBase.PlayerName);
            }
            else
            {
                Enemy enemy = (Enemy)characters[i];
                //Debug.Log(enemy.EnemyBase.EnemyName);
            }
        }
        // 一番早いキャラクターが動くキャラクターturnCharacter
        turnCharacterIndex = 0;
        turnCharacter = characters[turnCharacterIndex];
    }

    private void PrepareBattle()
    {
        bool[] prepareEnd = new bool[activePlayers.Count];

        // 指定の位置に移動する
        for (int i = 0; i < activePlayers.Count; i++)
        {
            prepareEnd[i] = PreparePlayerPosition(speed, activePlayers[i].PlayerUI.PlayerPos, activePlayers[i].PlayerModel, activePlayers[i].PlayerAnimator);
        }
        // 全員が移動し終わったら
        if (prepareEnd.All(val => val == true))
        {
            if (turnCharacter.isPlayer)
            {
                battleState = BattleState.PLAYER_MOVE;
            }
            else
            {
                battleState = BattleState.ENEMY_MOVE;
            }
            inputSkillStatement = InputSkillStatement.INIT_SKILL;
        }
    }

    private bool PreparePlayerPosition(float playerSpeed, Transform playerTargetPos, GameObject playerModel, Animator playerAnimator)
    {
        float directionX = playerTargetPos.position.x - playerModel.gameObject.transform.position.x;
        float directionZ = playerTargetPos.position.z - playerModel.gameObject.transform.position.z;

        Vector3 direction = new Vector3(directionX, 0, directionZ);
        playerModel.gameObject.transform.LookAt(playerTargetPos);
        Vector3 directionNormarized = direction.normalized;

        //Debug.Log("目指す場所" + playerTargetPos.position + "現在地" + playerModel.gameObject.transform.position);
        float diffX = playerTargetPos.position.x - playerModel.gameObject.transform.position.x;
        float diffZ = playerTargetPos.position.z - playerModel.gameObject.transform.position.z;
        Vector3 diff = new Vector3(diffX, 0, diffZ);
        if (diff.magnitude < 0.1f)
        {
            // アニメーションの終了
            playerAnimator.SetBool("RunToIdle", true);
            return true;
        }
        playerModel.transform.position += playerSpeed * Time.deltaTime * directionNormarized;

        return false;
    }

    // ==========================メインパネルからアクションを選択する
    private float animationTransitionTime = 0;

    private bool onceAnim;

    private void HandleActionSelection()
    {
        ///////////// アニメーションの指定 /////////////
        // 10秒立ったら
        // 10秒ごとにフラフラするアニメーション
        animationTransitionTime += Time.deltaTime;
        Player turnPlayer = (Player)turnCharacter;
        if (animationTransitionTime > 10 && animationTransitionTime < 11)
        {
            // 今選択しているキャラクターの待機モーションを変更する
            if (onceAnim == false)
            {
                turnPlayer.PlayerAnimator.SetTrigger("IdleToIdle2");
                onceAnim = true;
            }
        }
        if (animationTransitionTime > 20)
        {
            onceAnim = false;
            animationTransitionTime = 0;
        }

        ////////////////////////////////////////////
        // メインパネルの選択
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentMove < (int)Move.END - 1)
            {
                currentMove++;
                battleCommand.ActivateBattleSelectArrow((Move)currentMove);
            }
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentMove > 0)
            {
                currentMove--;
                battleCommand.ActivateBattleSelectArrow((Move)currentMove);
            }
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // UIの非表示
            battleCommand.ActivateBattleCommandPanel(false);
            PlayerMove();
        }
    }

    private void PlayerAction()
    {
        // メインパネルを表示する
        battleState = BattleState.PLAYER_ACTION_SELECT;
        Debug.Log("=====================" + battleState + "====================");

        battleCommand.ActivateBattleCommandPanel(true);

        //初期選択コマンド
        currentMove = (int)Move.ATTACK;
        battleCommand.ActivateBattleSelectArrow((Move)currentMove);
    }

    // スキルとかアイテム
    private void PlayerMove()
    {
        // 気絶しているなら
        if (TurnCharacter.isFainted == true)
        {
            // 復活モーション
            TurnCharacter.isFainted = false;
        }
        battleState = BattleState.PLAYER_MOVE;
        // Moveに応じたパネルを表示する
        if ((Move)currentMove == Move.ATTACK)
        {
            InitSkill();
            inputSkillStatement = ChangeInputSkillStatement();
        }
    }

    //  スキルの入力の状態を遷移する関数
    private InputSkillStatement ChangeInputSkillStatement()
    {
        InputSkillStatement nextStatement = inputSkillStatement;
        switch (inputSkillStatement)
        {
            case (InputSkillStatement.INIT_SKILL):
                nextStatement = InputSkillStatement.SKILL_SELECT;
                break;

            case (InputSkillStatement.SKILL_SELECT):
                nextStatement = InputSkillStatement.TARGET_SELECT;
                break;

            case (InputSkillStatement.TARGET_SELECT):
                nextStatement = InputSkillStatement.END_INPUT;
                break;

            case (InputSkillStatement.END_INPUT):
                Debug.Log("Statement Error");
                break;
        }
        Debug.Log(nextStatement);
        return nextStatement;
    }

    private void InitSkill()
    {
        Debug.Log("PlayerInitSkill");
        // スキルパネルを表示
        battleCommand.ActivateSkillCommandPanel(true);
        // 装備ペルソナの表示
        Player turnPlayer = (Player)TurnCharacter;
        Debug.Log(turnPlayer.PlayerUI.PlayerPersonaPos.childCount);
        // 前まで表示していたペルソナを消す
        if (turnPlayer.PlayerUI.PlayerPersonaPos.childCount != 0)
        {
            Destroy(turnPlayer.PlayerUI.PlayerPersonaPos.GetChild(0).gameObject);
        }
        // ここ　むきがおかしい
        GameObject playerPersona = Instantiate(turnPlayer.EquipEnemy.EnemyBase.EnemyModel, turnPlayer.PlayerUI.PlayerPersonaPos.position, new Quaternion(0, 0, 0, 0));
        playerPersona.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        playerPersona.transform.parent = turnPlayer.PlayerUI.PlayerPersonaPos;
        // アニメーターの取得
        turnPlayer.EquipEnemy.EnemyAnimator = playerPersona.GetComponent<Animator>();

        Vector3 targetPos = turnPlayer.PlayerUI.PlayerPos.position;
        targetPos = new Vector3(turnPlayer.PlayerUI.PlayerPos.position.x, turnPlayer.PlayerUI.PlayerPersonaPos.position.y - turnPlayer.PlayerUI.PlayerPos.position.y, turnPlayer.PlayerUI.PlayerPos.position.z);
        Debug.Log("playerPos" + turnPlayer.PlayerUI.PlayerPos.position.y);
        Debug.Log("personaPos" + turnPlayer.PlayerUI.PlayerPersonaPos.position.y);
        Debug.Log("targetPos:" + targetPos);
        playerPersona.transform.LookAt(targetPos);
        // EnhancedScrollerのデリゲートを指定する
        // デリゲートを設定することで、スクロールビューが必要な情報を取得
        playerSkillPanel.Delegate = this;
        // LoadSkillData(activePlayers[0].Skills);
        // 修正① そのターンのプレイヤーキャラのスキルをセットする
        Player player = (Player)TurnCharacter;
        LoadSkillData(player.Skills);
    }

    private void InitTarget(EnemySkill playerSkill)
    {
        selectedTargetIndex = 0;
        // 攻撃スキルだったら
        if (playerSkill.skillBase.IsAttackSkill)
        {
            // 全体攻撃だったら
            if (playerSkill.skillBase.IsAll)
            {
                selectedEnemies = new Enemy[activeEnemies.Count];
                for (int i = 0; i < activeEnemies.Count; i++)
                {
                    activeEnemies[i].EnemyUI.SelectedArrow.SetActive(true);
                    selectedEnemies[i] = activeEnemies[i];
                }
            }
            //　単体攻撃だったら
            else
            {
                activeEnemies[selectedTargetIndex].EnemyUI.SelectedArrow.SetActive(true);
                Debug.Log("selectedTargetIndex初期" + selectedTargetIndex);
            }
        }// 回復スキルだったら味方側が対象になる
        else if (playerSkill.skillBase.IsHeal)
        {
            // 対象が全体だったら
            if (playerSkill.skillBase.IsAll)
            {
                selectedPlayers = new Player[activePlayers.Count];
                for (int i = 0; i < activePlayers.Count; i++)
                {
                    activePlayers[i].PlayerUI.SetActiveSelectedArrow(true);
                    selectedPlayers[i] = activePlayers[i];
                }
            }
            //　対象が単体だったら
            else
            {
                activePlayers[selectedTargetIndex].PlayerUI.SetActiveSelectedArrow(true);
                Debug.Log("selectedTargetIndex初期" + selectedTargetIndex);
            }
        }
    }

    // ターゲットの選択
    private void HandleSelectTarget(EnemySkill skill)
    {
        bool selectionChanged = false;

        // 攻撃魔法だったら
        if (skill.skillBase.IsAttackSkill)
        {
            // 敵が単体だったら
            if (skill.skillBase.IsAll == false)
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    if (selectedTargetIndex > 0)
                    {
                        selectedTargetIndex--;
                    }
                    else
                    {
                        selectedTargetIndex = 0;
                    }
                    selectionChanged = true;

                    Debug.Log("selectedTargetIndex左-" + selectedTargetIndex);
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    if (selectedTargetIndex < activeEnemies.Count - 1)
                    {
                        selectedTargetIndex++;
                    }
                    selectionChanged = true;

                    Debug.Log("selectedTargetIndexみぎ+" + selectedTargetIndex);
                }

                if (selectionChanged == true)
                {
                    for (int i = 0; i < activeEnemies.Count; i++)
                    {
                        bool isActiveSelectedArrow = (i == selectedTargetIndex);
                        activeEnemies[i].EnemyUI.SelectedArrow.SetActive(isActiveSelectedArrow);
                    }

                    activeEnemies[selectedTargetIndex].EnemyUI.SelectedArrow.SetActive(true);

                    selectedEnemies = new Enemy[1];
                    selectedEnemies[0] = activeEnemies[selectedTargetIndex];
                }
            }
            else
            {
                selectedEnemies = new Enemy[activeEnemies.Count];
                selectedEnemies = activeEnemies.ToArray();
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                // 選択矢印を消す
                for (int i = 0; i < activeEnemies.Count; i++)
                {
                    Debug.Log(i + "," + activeEnemies.Count);
                    activeEnemies[i].EnemyUI.SelectedArrow.SetActive(false);
                }
                inputSkillStatement = InputSkillStatement.END_INPUT;
            }
        }
        // 回復魔法だったら
        else if (skill.skillBase.IsHeal)
        {
            // 対象が単体だったら
            if (skill.skillBase.IsAll == false)
            {
                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    if (selectedTargetIndex > 0)
                    {
                        selectedTargetIndex--;
                    }
                    else
                    {
                        selectedTargetIndex = 0;
                    }
                    selectionChanged = true;

                    Debug.Log("selectedTargetIndex左-" + selectedTargetIndex);
                }
                else if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    if (selectedTargetIndex < activePlayers.Count - 1)
                    {
                        selectedTargetIndex++;
                    }
                    selectionChanged = true;

                    Debug.Log("selectedTargetIndexみぎ+" + selectedTargetIndex);
                }

                if (selectionChanged == true)
                {
                    for (int i = 0; i < activePlayers.Count; i++)
                    {
                        bool isActiveSelectedArrow = (i == selectedTargetIndex);
                        activePlayers[i].PlayerUI.SelectedArrow.SetActive(isActiveSelectedArrow);
                    }

                    activePlayers[selectedTargetIndex].PlayerUI.SelectedArrow.SetActive(true);

                    selectedPlayers = new Player[1];
                    selectedPlayers[0] = activePlayers[selectedTargetIndex];
                }
            }
            else
            {
                selectedPlayers = new Player[activeEnemies.Count];
                selectedPlayers = activePlayers.ToArray();
            }
            if (Input.GetKeyDown(KeyCode.Return))
            {
                // 選択矢印を消す
                for (int i = 0; i < activePlayers.Count; i++)
                {
                    Debug.Log(i + "," + activePlayers.Count);
                    activePlayers[i].PlayerUI.SelectedArrow.SetActive(false);
                }
                inputSkillStatement = InputSkillStatement.END_INPUT;
            }
        }
    }

    private void PlayerEndSkillInput()
    {
        Debug.Log("PlayerEndInput()");

        StartCoroutine(PerformPlayerSkill());
    }

    // 全体攻撃のモーションを変更中
    // スキルの発動
    private IEnumerator PerformPlayerSkill()
    {
        hitWeakFirst = false;
        battleState = BattleState.BUSY;
        // 技を決定
        Player player = (Player)TurnCharacter;
        EnemySkill playerSkill = player.Skills[selectedSkillIndex];
        dialogBox.SetMessage("PlayerTurn " + playerSkill.skillBase.SkillName);
        Debug.Log("======" + player.PlayerBase.PlayerName + "PlayerTurn " + playerSkill.skillBase.SkillName + "======");

        Debug.Log("名前" + player.PlayerBase.PlayerName + "発動スキル" + playerSkill.skillBase.SkillName);
        // 消費SP分減らす
        player.UseSp(playerSkill);
        player.PlayerUI.UpdateHpSp();
        // 攻撃魔法だったら
        if (playerSkill.skillBase.IsAttackSkill)
        {
            // ダメージ決定　
            bool[] isDying = new bool[activeEnemies.FindAll(value => value.isDying == false).Count];

            // ターンのプレイヤーのスキル発動モーション
            player.PlayerAnimator.Play(hashSkill);
            yield return null;// ステートの反映
            // ターンのプレイヤーのペルソナのスキル発動モーション
            player.EquipEnemy.EnemyAnimator.Play(hashAttack);
            yield return null;// ステートの反映
            yield return new WaitForAnimation(player.PlayerAnimator, 0);
            player.PlayerAnimator.SetBool("SkillToIdle", true);
            player.PlayerAnimator.SetBool("IdleToSkillIdle", false);
            // スキルを発動し終わったらペルソナが消える
            if (player.PlayerUI.PlayerPersonaPos.childCount > 0)
            {
                Destroy(player.PlayerUI.PlayerPersonaPos.GetChild(0).gameObject);
            }
            Debug.Log(activeEnemies.Count);
            // 全体攻撃だったら
            if (playerSkill.skillBase.IsAll)
            {
                // アニメーションやUI表示
                for (int i = 0; i < activeEnemies.Count; i++)
                {
                    // 無効じゃなければ
                    if (!activeEnemies[i].isInvalid(playerSkill))
                    {
                        // 敵にスキル発動モーション
                        Instantiate(playerSkill.skillBase.SkillRecieveEffect, activeEnemies[i].EnemyModel.transform.position, activeEnemies[i].EnemyModel.transform.rotation);
                    }
                }
                for (int i = 0; i < activeEnemies.Count; i++)
                {
                    // 弱点なら
                    if (activeEnemies[i].isEffective(playerSkill))
                    {
                        // 気絶モーション
                    }
                    // 無効なら
                    else if (activeEnemies[i].isInvalid(playerSkill))
                    {
                        // 無効UI表示
                        activeEnemies[i].EnemyUI.SetActiveInvalidInfo();
                    }
                    else
                    {
                        // ダメージモーション　敵のアニメーターにダメージのステート追加
                        activeEnemies[i].EnemyAnimator.Play(hashDamage);
                        yield return null;// ステートの反映
                    }
                }
                // 一体（回）分だけ待つ
                yield return new WaitForAnimation(activeEnemies[0].EnemyAnimator, 0);

                for (int i = 0; i < activeEnemies.Count; i++)
                {
                    // ダメージ　
                    isDying[i] = activeEnemies[i].TakeDamage(playerSkill, (Player)TurnCharacter);

                    // 弱点だったら
                    if (activeEnemies[i].isEffective(playerSkill))
                    {
                        // 初めて弱点をついたら
                        if (characters.Find(value => value == activeEnemies[i]).isFainted == false)
                        {
                            hitWeakFirst = true;
                        }
                        // 弱点をつかれたので気絶
                        characters.Find(value => value == activeEnemies[i]).isFainted = true;

                        //battleEnemyUIs[i].SetActiveWeakInfo();
                        activeEnemies[i].EnemyUI.SetActiveWeakInfo();
                    }
                }

                // HPSPの反映
                for (int i = 0; i < activeEnemies.Count; i++)
                {
                    activeEnemies[i].EnemyUI.UpdateHp();
                }
            }
            else//単体攻撃だったら
            {
                if (!activeEnemies[selectedTargetIndex].isInvalid(playerSkill))
                {
                    if (playerSkill.skillBase.SkillRecieveEffect != null)
                    {
                        Instantiate(playerSkill.skillBase.SkillRecieveEffect, activeEnemies[selectedTargetIndex].EnemyModel.transform.position, activeEnemies[selectedTargetIndex].EnemyModel.transform.rotation);
                    }
                }
                isDying[selectedTargetIndex] = activeEnemies[selectedTargetIndex].TakeDamage(playerSkill, activePlayers[0]);
                // アニメーションやUI表示
                // 弱点なら
                if (activeEnemies[selectedTargetIndex].isEffective(playerSkill))
                {
                    // 気絶モーション
                }
                else if (activeEnemies[selectedTargetIndex].isInvalid(playerSkill))
                {
                    // 無効UI表示
                    activeEnemies[selectedTargetIndex].EnemyUI.SetActiveInvalidInfo();
                }
                else
                {
                    // ダメージモーション
                    activeEnemies[selectedTargetIndex].EnemyAnimator.Play(hashDamage);
                    yield return null;// ステートの反映
                }

                // 弱点をついたら
                if (activeEnemies[selectedTargetIndex].isEffective(playerSkill))
                {
                    // 初めて弱点をついたら
                    if (characters.Find(value => value == activeEnemies[selectedTargetIndex]).isFainted == false)
                    {
                        hitWeakFirst = true;
                    }
                    // 弱点をつかれたので敵は気絶
                    characters.Find(value => value == activeEnemies[selectedTargetIndex]).isFainted = true;
                    activeEnemies[selectedTargetIndex].EnemyUI.SetActiveWeakInfo();
                }

                // HPSPの反映
                for (int i = 0; i < activeEnemies.Count; i++)
                {
                    activeEnemies[i].EnemyUI.UpdateHp();
                }
            }

            for (int i = 0; i < activeEnemies.Count; i++)
            {
                // 戦闘不能な敵を消す
                if (isDying[i] == true)
                {
                    // i番目の敵のモデルを消す
                    //activeEnemies[i].EnemyModel.SetActive(false);
                    Destroy(activeEnemies[i].EnemyModel);
                    // i番目の敵のUIを消す
                    activeEnemies[i].EnemyUI.UnActiveUIPanel();
                    // i番目の敵のisDyingをtrueにする
                    characters.Find(value => value == activeEnemies[i]).isDying = true;
                }
            }
            // 全員戦闘不能ならメッセージ
            // 戦闘不能の敵を検索してリムーブする
            List<Enemy> deadEnemies = activeEnemies.FindAll(value => value.isDying == true);
            for (int i = 0; i < deadEnemies.Count; i++)
            {
                activeEnemies.Remove(deadEnemies[i]);
            }

            Debug.Log("EnemySkill　1秒まつ");
            yield return new WaitForSeconds(1);
            // 全員瀕死になったら戦闘不能
            if (isDying.All(value => value == true))
            {
                Debug.Log("戦闘不能");

                battleState = BattleState.BUSY;
                yield return new WaitForSeconds(0.7f);
                // 3Dモデルの削除
                for (int i = 0; i < activePlayers.Count; i++)
                {
                    Destroy(activePlayers[i].PlayerModel);
                }
                //フィールドのシーンに戻る
                gameManager.EndBattle();
            }
            else// 一体でも生き残っていれば
            {
                // 弱点をついたらもう１ターン
                if (hitWeakFirst)
                {
                    Debug.Log("弱点をついたのでもう１ターン");
                    // battleStateの更新
                    if (TurnCharacter.isPlayer)
                    {
                        battleState = BattleState.PLAYER_MOVE;
                        inputSkillStatement = InputSkillStatement.INIT_SKILL;
                        Debug.Log(battleState);
                    }
                    else
                    {
                        battleState = BattleState.ENEMY_MOVE;
                        inputSkillStatement = InputSkillStatement.INIT_SKILL;
                        Debug.Log(battleState);
                    }
                }
                else
                {
                    NextTurn();
                }
            }
        }
        // 回復魔法だったら
        else if (playerSkill.skillBase.IsHeal)
        {
            // ターンのプレイヤーのスキル発動モーション
            player.PlayerAnimator.Play(hashSkill);
            yield return null;// ステートの反映
            yield return new WaitForAnimation(player.PlayerAnimator, 0);
            player.PlayerAnimator.SetBool("SkillToIdle", true);
            player.PlayerAnimator.SetBool("IdleToSkillIdle", false);

            // 全体回復だったら
            if (playerSkill.skillBase.IsAll)
            {
                for (int i = 0; i < activePlayers.Count; i++)
                {
                    // 味方にスキル発動モーション
                    Instantiate(playerSkill.skillBase.SkillRecieveEffect, activePlayers[i].PlayerModel.transform.position, activePlayers[i].PlayerModel.transform.rotation);
                }
                for (int i = 0; i < activePlayers.Count; i++)
                {
                    // 回復モーション ここがだめ
                    activePlayers[i].PlayerAnimator.Play(hashHeal);
                    activePlayers[i].PlayerAnimator.SetBool("HealToIdle", true);
                    yield return null;// ステートの反映
                }
                // 一体（回）分だけ待つ
                yield return new WaitForAnimation(activePlayers[0].PlayerAnimator, 0);
                for (int i = 0; i < activePlayers.Count; i++)
                {
                    // 回復
                    activePlayers[i].TakeHeal(playerSkill, (Player)TurnCharacter);
                }

                // HPSPの反映
                for (int i = 0; i < activePlayers.Count; i++)
                {
                    activePlayers[i].PlayerUI.UpdateHpSp();
                }
            }
            else//対象が単体だったら
            {
                Instantiate(playerSkill.skillBase.SkillRecieveEffect, activePlayers[selectedTargetIndex].PlayerModel.transform.position, activePlayers[selectedTargetIndex].PlayerModel.transform.rotation);
                // ここ
                activePlayers[selectedTargetIndex].TakeHeal(playerSkill, activePlayers[selectedTargetIndex]);

                // 回復モーション
                activePlayers[selectedTargetIndex].PlayerAnimator.Play(hashHeal);
                activePlayers[selectedTargetIndex].PlayerAnimator.SetBool("HealToIdle", true);
                yield return null;// ステートの反映
                yield return new WaitForAnimation(activePlayers[selectedTargetIndex].PlayerAnimator, 0);

                // HPSPの反映
                for (int i = 0; i < activePlayers.Count; i++)
                {
                    activePlayers[i].PlayerUI.UpdateHpSp();
                }
            }
            NextTurn();
        }
    }

    private void NextTurn()
    {
        if (turnCharacterIndex < characters.Count - 1)
        {
            turnCharacterIndex++;
        }
        else
        {
            turnCharacterIndex = 0;
        }
        TurnCharacter = characters[turnCharacterIndex];

        while (TurnCharacter.isDying == true) // 戦闘不能だったら飛ばす
        {
            if (turnCharacterIndex < characters.Count - 1)
            {
                turnCharacterIndex++;
            }
            else
            {
                turnCharacterIndex = 0;
            }
            TurnCharacter = characters[turnCharacterIndex];
        }

        // battleStateの更新
        if (TurnCharacter.isPlayer)
        {
            battleState = BattleState.PLAYER_MOVE;
            inputSkillStatement = InputSkillStatement.INIT_SKILL;
            Debug.Log(battleState);
        }
        else
        {
            battleState = BattleState.ENEMY_MOVE;
            inputSkillStatement = InputSkillStatement.INIT_SKILL;
            Debug.Log(battleState);
        }
        Debug.Log("=====================" + battleState + "====================");
        //Debug.Log()
        Debug.Log("順番のキャラクター" + turnCharacterIndex + "生きてるキャラクターの数" + characters.Count(value => value.isDying == false));
    }

    // 敵============================
    private void EnemyMove()
    {
        // 気絶しているなら
        if (TurnCharacter.isFainted == true)
        {
            // 復活モーション
            TurnCharacter.isFainted = false;
        }
        inputSkillStatement = ChangeInputSkillStatement();
    }

    private void EnemySkillSelect()
    {
        Enemy turnEnemy = (Enemy)characters[turnCharacterIndex];
        selectedSkillIndex = Random.Range(0, turnEnemy.Skills.Count);
        inputSkillStatement = ChangeInputSkillStatement();
        Debug.Log(inputSkillStatement);
    }

    private void EnemyTargetSelect(EnemySkill enemySkill)
    {
        //Debug.Log("EnemyTargetSelect()");
        // ターゲットをランダムで選択
        // 単体だったら Random.Range(min,max) 最大値を含まない
        if (enemySkill.skillBase.IsAttackSkill)
        {
            selectedTargetIndex = Random.Range(0, activePlayers.Count);
        }
        else
        {
            selectedTargetIndex = Random.Range(0, activeEnemies.Count);
        }
        inputSkillStatement = ChangeInputSkillStatement();
    }

    private void EnemyEndInput()
    {
        Enemy turnEnemy = (Enemy)characters[turnCharacterIndex];
        StartCoroutine(PerformEnemySkill(turnEnemy.Skills[selectedSkillIndex]));
    }

    // 敵のスキルの発動
    private IEnumerator PerformEnemySkill(EnemySkill enemySkill)
    {
        Enemy tmpenemy2 = (Enemy)turnCharacter;
        Debug.Log("名前" + tmpenemy2.EnemyBase.EnemyName + "発動スキル" + enemySkill.skillBase.SkillName);
        battleState = BattleState.BUSY;
        // 攻撃魔法だったら
        if (enemySkill.skillBase.IsAttackSkill)
        {
            hitWeakFirst = false;
            // デバッグのためのやつ
            Enemy enemy = (Enemy)characters[turnCharacterIndex];
            dialogBox.SetMessage(enemy.EnemyBase.EnemyName + "EnemyTurn " + enemySkill.skillBase.SkillName);
            Debug.Log("======" + enemy.EnemyBase.EnemyName + "EnemyTurn " + enemySkill.skillBase.SkillName + "======");

            // ダメージ決定 修正 playerUnit.Players.Length→ activePlayers.Count
            bool[] isDying = new bool[activePlayers.FindAll(value => value.isDying == false).Count];

            // ターン中の敵のスキル発動モーション
            enemy.EnemyAnimator.Play(hashAttack);
            yield return null;// ステートの反映
            yield return new WaitForAnimation(enemy.EnemyAnimator, 0);

            // 全体選択なら
            if (enemySkill.skillBase.IsAll)
            {
                for (int i = 0; i < activePlayers.Count; i++)
                {
                    // 無効じゃなければ
                    if (!activePlayers[i].isInvalid(enemySkill))
                    {
                        // 敵にスキル発動モーション
                        Instantiate(enemySkill.skillBase.SkillRecieveEffect, activePlayers[i].PlayerModel.transform.position, activePlayers[i].PlayerModel.transform.rotation);
                    }
                }

                // アニメーションやUI表示
                for (int i = 0; i < activePlayers.Count; i++)
                {
                    // 弱点なら
                    if (activePlayers[i].isEffective(enemySkill))
                    {
                        // 気絶モーション
                    }
                    else if (activePlayers[i].isInvalid(enemySkill))
                    {
                        // 無効UI表示
                        activePlayers[i].PlayerUI.SetActiveInvalidInfo();
                    }
                    else
                    {
                        // ダメージモーション　敵のアニメーターにダメージのステート追加
                        activePlayers[i].PlayerAnimator.Play(hashDamage);
                        yield return null;// ステートの反映
                    }
                }
                // 一体（回）分だけ待つ
                yield return new WaitForAnimation(activePlayers[0].PlayerAnimator, 0);
                for (int i = 0; i < activePlayers.Count; i++)
                {
                    // 修正　activeEnemies[0]
                    isDying[i] = activePlayers[i].TakeDamage(enemySkill, (Enemy)TurnCharacter);
                    // 弱点をついたら
                    if (activePlayers[i].isEffective(enemySkill))
                    {
                        // 弱点をついて初めて気絶させる
                        if (characters.Find(value => value == activePlayers[i]).isFainted == false)
                        {
                            hitWeakFirst = true;
                        }
                        // 弱点をつかれたので気絶
                        characters.Find(value => value == activePlayers[i]).isFainted = true;
                        activePlayers[i].PlayerUI.SetActiveWeakInfo();
                    }
                    // HPSPの反映
                    activePlayers[i].PlayerUI.UpdateHpSp();
                }
            }
            else// 単体選択なら
            {
                isDying[selectedTargetIndex] = activePlayers[selectedTargetIndex].TakeDamage(enemySkill, activeEnemies[0]);

                // アニメーションやUI表示

                // 無効じゃなければ
                if (!activePlayers[selectedTargetIndex].isInvalid(enemySkill))
                {
                    // 敵にスキル発動モーション
                    if (enemySkill.skillBase.SkillRecieveEffect != null)
                    {
                        Instantiate(enemySkill.skillBase.SkillRecieveEffect, activePlayers[selectedTargetIndex].PlayerModel.transform.position, activePlayers[selectedTargetIndex].PlayerModel.transform.rotation);
                    }
                }
                // 弱点なら
                if (activePlayers[selectedTargetIndex].isEffective(enemySkill))
                {
                    // 気絶モーション
                }
                else if (activePlayers[selectedTargetIndex].isInvalid(enemySkill))
                {
                    // 無効UI表示
                    activePlayers[selectedTargetIndex].PlayerUI.SetActiveInvalidInfo();
                }
                else
                {
                    // ダメージモーション
                    activePlayers[selectedTargetIndex].PlayerAnimator.Play(hashDamage);
                    yield return null;// ステートの反映
                }

                // 状態の変更
                if (activePlayers[selectedTargetIndex].isEffective(enemySkill))
                {
                    //battlePlayerUIs[selectedTargetIndex].SetActiveWeakInfo();
                    activePlayers[selectedTargetIndex].PlayerUI.SetActiveWeakInfo();
                    if (characters.Find(value => value == activePlayers[selectedTargetIndex]).isFainted == false)
                    {
                        hitWeakFirst = true;
                    }
                    // 弱点をつかれたので気絶
                    characters.Find(value => value == activePlayers[selectedTargetIndex]).isFainted = true;
                }

                //battlePlayerUIs[selectedTargetIndex].UpdateHpSp();
                activePlayers[selectedTargetIndex].PlayerUI.UpdateHpSp();
            }

            Debug.Log("EnemySkill　1秒まつ");
            yield return new WaitForSeconds(1);
            // 全員戦闘不能ならメッセージ
            for (int i = 0; i < isDying.Length; i++)
            {
                // 戦闘不能なら
                if (isDying[i] == true)
                {
                    // 戦闘不能モーション

                    // リムーブ
                    //characters[i].isDying = true; // 戦闘不能
                    characters.Find(value => value == activePlayers[i]).isDying = true;

                    // デバッグよう
                    Player faintedPlayer = (Player)characters.Find(value => value == activePlayers[i]);
                    Debug.Log(faintedPlayer.PlayerBase.PlayerName + "は戦闘不能" + faintedPlayer.isDying);
                    activePlayers.Remove(faintedPlayer);
                    for (int j = 0; j < activePlayers.Count; j++)
                    {
                        Debug.Log("残りの敵" + activePlayers[j].PlayerBase.PlayerName); ;
                    }
                }
            }

            if (isDying.All(value => value == true))
            {
                Debug.Log("戦闘不能");
                yield return new WaitForSeconds(0.7f);
            }
            else
            {
                // 弱点をついたらもう１ターン
                if (hitWeakFirst)
                {
                    Debug.Log("弱点をついたのでもう１ターン");
                    if (TurnCharacter.isPlayer)
                    {
                        // デバッグよう
                        Player tmpplayer = (Player)characters[turnCharacterIndex];
                        Debug.Log("今のturnCharacterIndex" + turnCharacterIndex + "キャラクターネーム" + tmpplayer.PlayerBase.PlayerName);
                        battleState = BattleState.PLAYER_MOVE;
                        inputSkillStatement = InputSkillStatement.INIT_SKILL;
                    }
                    else
                    {
                        // デバッグよう
                        Enemy tmpenemy = (Enemy)characters[turnCharacterIndex];
                        Debug.Log("今のturnCharacterIndex" + turnCharacterIndex + "キャラクターネーム" + tmpenemy.EnemyBase.EnemyName);

                        battleState = BattleState.ENEMY_MOVE;
                        inputSkillStatement = InputSkillStatement.INIT_SKILL;
                    }
                }
                else
                {
                    NextTurn();
                }
            }
        }
        // 回復魔法だったら
        else if (enemySkill.skillBase.IsHeal)
        {
            Enemy enemy = (Enemy)characters[turnCharacterIndex];
            // 入力側のスキルのアニメーションを再生
            enemy.EnemyAnimator.Play(hashSkill);
            yield return null;// ステートの反映
            yield return new WaitForAnimation(enemy.EnemyAnimator, 0);

            // 全体選択なら
            if (enemySkill.skillBase.IsAll)
            {
                // 受ける側のスキルのアニメーションを再生
                for (int i = 0; i < activeEnemies.Count; i++)
                {
                    activeEnemies[i].EnemyAnimator.Play(hashHeal);
                    yield return null;// ステートの反映
                    Instantiate(enemySkill.skillBase.SkillRecieveEffect, activeEnemies[i].EnemyModel.transform.position, activeEnemies[i].EnemyModel.transform.rotation);
                }
                for (int i = 0; i < activeEnemies.Count; i++)
                {
                    // 回復する
                    activeEnemies[i].TakeHeal(enemySkill);
                    activeEnemies[i].EnemyUI.UpdateHp();
                }
                yield return new WaitForAnimation(activeEnemies[0].EnemyAnimator, 0);
            }
            else
            {
                Instantiate(enemySkill.skillBase.SkillRecieveEffect, activeEnemies[selectedTargetIndex].EnemyModel.transform.position, activeEnemies[selectedTargetIndex].EnemyModel.transform.rotation);
                // ここ
                activeEnemies[selectedTargetIndex].TakeHeal(enemySkill);

                // 回復モーション
                activeEnemies[selectedTargetIndex].EnemyAnimator.Play(hashHeal);
                //activeEnemies[selectedTargetIndex].EnemyAnimator.SetBool("HealToIdle", true);
                yield return null;// ステートの反映
                yield return new WaitForAnimation(activeEnemies[selectedTargetIndex].EnemyAnimator, 0);

                // HPの反映
                for (int i = 0; i < activeEnemies.Count; i++)
                {
                    activeEnemies[i].EnemyUI.UpdateHp();
                }
            }
            NextTurn();
        }
    }

    /// ===================================

    // スキルパネルからアクションを選択する
    private void HandleSkillSelection()
    {
        ///////////// アニメーションの指定 /////////////
        // 10秒立ったら
        // 10秒ごとにフラフラするアニメーション
        Player turnPlayer = (Player)TurnCharacter;
        turnPlayer.PlayerAnimator.SetBool("IdleToSkillIdle", true);
        ////////////////////////////////////////////////////
        bool selectionChanged = false;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            // 指定された値を範囲内にクランプ（制約）するために使用されるUnityの関数です。
            // 値が指定された範囲内にある場合はそのまま返し、範囲外の場合は範囲の最小値または最大値に
            // 制約されます
            selectedSkillIndex = Mathf.Clamp(selectedSkillIndex - 1, 0, skillDatas.Count - 1);
            selectionChanged = true;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedSkillIndex = Mathf.Clamp(selectedSkillIndex + 1, 0, skillDatas.Count - 1);
            selectionChanged = true;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            // 右を押したらペルソナの付け替え　下++
            if (turnPlayer.EnemyList.Count > 1)
            {
                if (turnPlayer.EquipPersonaIndex < turnPlayer.EnemyList.Count - 1)
                {
                    Debug.Log(turnPlayer.EnemyList.Count + "count:index" + turnPlayer.EquipPersonaIndex);
                    // 装備ペルソナの変更
                    turnPlayer.EquipPersonaIndex++;
                    // スキルのリロード
                    InitSkill();
                    // UIの表示
                    selectionChanged = true;
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (turnPlayer.EquipPersonaIndex > 0)
            {
                // 装備ペルソナの変更
                turnPlayer.EquipPersonaIndex--;
                // スキルのリロード
                InitSkill();
                // UIの表示
                selectionChanged = true;
            }
        }

        // 技決定
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // 技決定の処理
            if (turnPlayer.Sp >= turnPlayer.Skills[selectedSkillIndex].skillBase.Sp)
            {
                // UIを非表示
                battleCommand.ActivateSkillCommandPanel(false);
                InitTarget(turnPlayer.Skills[selectedSkillIndex]);
                //ここ
                Debug.Log("activePlayers" + activePlayers.Count + "selectedTargetIndex" + selectedTargetIndex);

                inputSkillStatement = ChangeInputSkillStatement();
            }
            else
            {
                // 選択できないよ
            }
        }

        // 選択中
        if (selectionChanged)
        {
            // 選択されたインデックスが更新されたから基礎データを更新する
            for (int i = 0; i < skillDatas.Count; i++)
            {
                // 選択中のisSelectedをtrueにする
                skillDatas[i].isSelected = (i == selectedSkillIndex);
            }

            // アクティブセルに対してUIの更新をする
            playerSkillPanel.RefreshActiveCellViews();
            // 選択されたインデックスが最下部またはその先にある時
            if (selectedSkillIndex >= playerSkillPanel.EndCellViewIndex)
            {
                /// <summary>
                /// Jump to a position in the playerSkillPanel based on a dataIndex. This overload allows you
                /// to specify a specific offset within a cell as well.
                /// dataIndex に基づいて、スクローラー内のある位置にジャンプする。
                /// このオーバーロードでは、セル内の特定のオフセットも指定することができます。
                /// </summary>
                /// <param name="dataIndex">ジャンプ先のdataIndex</param>int
                /// <param name="playerSkillPanelOffset">スクローラーの開始位置（上／左）からのオフセット（0～1）。float
                /// この範囲外は、スクローラーの表示可能領域の前後の位置にジャンプします</param>float
                /// <param name="cellOffset">セルの先頭（上／左）からのオフセット（0～1）。</param>bool
                /// <param name="useSpacing">ジャンプでスクロールの間隔を計算するかどうか</param>TweenType
                /// <param name="tweenType">ジャンプに使用するイージングについて</param>float
                /// <param name="tweenTime">ジャンプポイントまでの補間時間</param>float
                /// <param name="jumpComplete">このデリゲートは、ジャンプが完了したときに起動されます</param>Action
                playerSkillPanel.JumpToDataIndex(selectedSkillIndex, 1.0f, 1.0f);
            }
            else if (selectedSkillIndex <= playerSkillPanel.StartCellViewIndex)
            {
                // 選択されたインデックスが最上部またはそれ以上にある時
                playerSkillPanel.JumpToDataIndex(selectedSkillIndex, 0.0f, 0.0f);
            }
        }
    }

    /// ===================================

    // スキルデータの設定

    private void LoadSkillData(List<EnemySkill> playerSkillDatas)
    {
        // 適当なデータを設定する
        skillDatas = new List<SkillCellData>();
        for (int i = 0; i < playerSkillDatas.Count; i++)
        {
            skillDatas.Add(new SkillCellData()
            {
                skillText = playerSkillDatas[i].skillBase.SkillName,
                isSelected = i == selectedSkillIndex,
                type = playerSkillDatas[i].skillBase.MagicType,
                sp = playerSkillDatas[i].skillBase.Sp
            });
        }

        // データが揃ったのでスクローラーをリロードする
        playerSkillPanel.ReloadData();
    }

    public int GetNumberOfCells(EnhancedScroller playerSkillPanel)
    {
        return skillDatas.Count;
    }

    public float GetCellViewSize(EnhancedScroller playerSkillPanel, int dataIndex)
    {
        return cellSize;
    }

    // 表示するセルを取得します
    public EnhancedScrollerCellView GetCellView(EnhancedScroller playerSkillPanel, int dataIndex, int cellIndex)
    {
        SkillCellView cellView = playerSkillPanel.GetCellView(cellViewPrefab) as SkillCellView;
        cellView.name = "Cell" + dataIndex.ToString();
        cellView.SetSkillData(skillDatas[dataIndex]);

        return cellView;
    }
}

// カスタムコルーチン：現在のアニメーションステートが再生し終わるまで待つ
public class WaitForAnimation : CustomYieldInstruction
{
    private Animator m_animator;
    private int m_lastStateHash = 0;
    private int m_layerNo = 0;

    public WaitForAnimation(Animator animator, int layerNo)
    {
        Init(animator, layerNo, animator.GetCurrentAnimatorStateInfo(layerNo).fullPathHash);
    }

    private void Init(Animator animator, int layerNo, int hash)
    {
        m_layerNo = layerNo;
        m_animator = animator;
        m_lastStateHash = hash;
    }

    public override bool keepWaiting
    {
        get
        {
            var currentAnimatorState = m_animator.GetCurrentAnimatorStateInfo(m_layerNo);
            // WaitForAnimation()が実行された時のアニメーションステートと現在のアニメーションステートが同じ
            // かつ，現在のアニメーションステートが終わったら trueを返してコルーチン終了
            return currentAnimatorState.fullPathHash == m_lastStateHash && (currentAnimatorState.normalizedTime < 1);
        }
    }
}