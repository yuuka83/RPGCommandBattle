using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemyUnit : MonoBehaviour
{
    [SerializeField]
    private EnemyBase[] enemyBasies;

    [SerializeField]
    private GameObject[] enemyPos3;

    private Enemy[] enemies;

    public Enemy[] Enemies { get => enemies; }

    public void SetUp(int lowLevel, int highLevel)
    {
        int enemyNum = 3; /*= Random.Range(1,3);*/
        // レベル帯1-3のダンジョン
        int enemyKindNum;

        enemies = new Enemy[enemyNum];
        // 順番にソート
        Dictionary<Enemy, int> agiEnemyDic = new Dictionary<Enemy, int>();
        //enemyModels = new GameObject[enemyNum];
        for (int i = 0; i < enemyNum; i++)
        {
            enemyKindNum = Random.Range(0, enemyBasies.Length);
            Enemy enemy = new Enemy(enemyBasies[enemyKindNum], Random.Range(lowLevel, highLevel));
            agiEnemyDic.Add(enemy, enemy.EnemyBase.Agi);
        }

        int j = 0;
        // 敵の数が3体だったら
        if (enemyNum == 3)
        {
            foreach (var enemy in agiEnemyDic.OrderByDescending(c => c.Value))
            {
                enemies[j] = enemy.Key;
                enemies[j].EnemyModel = Instantiate(enemies[j].EnemyBase.EnemyModel, enemyPos3[j].transform);
                enemies[j].EnemyAnimator = enemies[j].EnemyModel.GetComponent<Animator>();
                //enemies[j].EnemyUIObject = enemyPos3[j].transform.Find("Child/EnemyCanvas").gameObject;
                enemies[j].EnemyUI = enemyPos3[j].transform.Find("EnemyCanvas").gameObject.GetComponent<BattleEnemyUI>();
                enemies[j].EnemyUI.SetEnemyData(enemies[j]);
                enemies[j].EnemyUI.ActivateUIPanel();
                j++;
            }
        }
    }
}