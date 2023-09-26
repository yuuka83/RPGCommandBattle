using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Player : Character
{
    public PlayerBase PlayerBase { get; set; }
    public Animator PlayerAnimator { get; set; }
    public GameObject PlayerModel { get; set; }
    public BattlePlayerUI PlayerUI { get; set; }

    [SerializeField]
    private ResultPlayerUI resultPanelUI;

    public ResultPlayerUI ResultPlayerUI { get => resultPanelUI; set => resultPanelUI = value; }

    public List<EnemySkill> Skills
    { get; set; }//所持ペルソナのスキル

    private ExpSheet expSheet;// 経験値表
    public int Level { get; set; }
    public int Hp { get; set; }
    public int Sp { get; set; }

    public int Exp
    {
        get; set;
    }

    private int nextExp;// 次のレベルまでの経験値

    // 所持ペルソナ
    public List<Enemy> EnemyList { get; set; }

    // 使用ペルソナ
    private int equipPersonaIndex;

    private Enemy equipEnemy;

    // コンストラクタ
    public Player(PlayerBase pBase, int level, List<Enemy> enemyList)
    {
        // しょきか
        isPlayer = true;
        PlayerBase = pBase;
        Level = level;
        // 修正する必要あり　その時点でのHPSP
        Hp = MaxHp;
        Sp = MaxSp;
        Exp = 0;
        expSheet = (ExpSheet)Resources.Load("levelExp");
        NextExp = expSheet.sheets[0].list[level - 1].nextExp;
        EnemyList = enemyList;
        equipPersonaIndex = 0;
        equipEnemy = enemyList[EquipPersonaIndex];
        Skills = new List<EnemySkill>();

        // 覚える技のレベル以上なら所持ペルソナのスキルをskillsに追加
        foreach (LearnableSkill learablePlayerSkill in EnemyList[0].EnemyBase.LearableEnemySkills)
        {
            if (Level >= learablePlayerSkill.Level)
            {
                Skills.Add(new EnemySkill(learablePlayerSkill.SkillBase));
            }
            // 8つ以上はだめ
            if (Skills.Count >= 8)
            {
                break;
            }
        }
    }

    // Levelに応じたステータスを返すもの:プロパティ
    public int MaxHp
    {
        get { return Mathf.FloorToInt((PlayerBase.PlayerMaxHp * Level) / 100f) + 10; }
    }

    public int MaxSp
    {
        get { return Mathf.FloorToInt((PlayerBase.PlayerMaxSp * Level) / 100f) + 30; }
    }

    public Enemy EquipEnemy { get => equipEnemy; }

    public int EquipPersonaIndex
    {
        get => equipPersonaIndex;
        set
        {
            equipPersonaIndex = value;
            equipEnemy = EnemyList[equipPersonaIndex];
            // Skillsを初期化
            Skills = new List<EnemySkill>();
            foreach (LearnableSkill learablePlayerSkill in EnemyList[equipPersonaIndex].EnemyBase.LearableEnemySkills)
            {
                if (Level >= learablePlayerSkill.Level)
                {
                    Skills.Add(new EnemySkill(learablePlayerSkill.SkillBase));
                }
                // 8つ以上はだめ
                if (Skills.Count >= 8)
                {
                    break;
                }
            }
        }
    }

    public int NextExp { get => nextExp; set => nextExp = value; }

    public bool isResist(EnemySkill enemySkill)
    {
        for (int i = 0; i < this.PlayerBase.ResistanceTypes.Length; i++)
        {
            // 使われたスキルが耐性だったら
            if (enemySkill.skillBase.MagicType == this.PlayerBase.ResistanceTypes[i])
            {
                return true;
            }
        }
        return false;
    }

    public bool isEffective(EnemySkill enemySkill)
    {
        for (int i = 0; i < this.PlayerBase.WeakTypes.Length; i++)
        {
            if (enemySkill.skillBase.MagicType == this.PlayerBase.WeakTypes[i])
            {
                return true;
            }
        }
        return false;
    }

    public bool isInvalid(EnemySkill enemySkill)
    {
        for (int i = 0; i < this.PlayerBase.InvalidTypes.Length; i++)
        {
            if (enemySkill.skillBase.MagicType == this.PlayerBase.InvalidTypes[i])
            {
                return true;
            }
        }
        return false;
    }

    private float critical = 1;

    public bool isCritical()
    {
        if (Random.value * 100 < 6.25)
        {
            critical = 2f;
            return true;
        }
        return false;
    }

    public bool TakeDamage(EnemySkill enemySkill, Enemy enemy)
    {
        // クリティカル

        // 相性
        float effectiveness = 1;// 効果量
        if (isEffective(enemySkill) == true)
        {
            effectiveness = 1.5f;
        }
        else if (isResist(enemySkill) == true)
        {
            effectiveness = 0.5f;
        }
        else if (isInvalid(enemySkill) == true)
        {
            effectiveness = 0;
        }

        float modifiers = Random.Range(0.85f, 1.0f) * effectiveness * critical;
        float a = (2 * enemy.Level + 10) / 250f;
        float d = a * enemySkill.skillBase.Power * ((float)enemy.MagicPower / equipEnemy.Def) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        Hp -= damage;
        if (Hp <= 0)
        {
            Hp = 0;
            return true;
        }
        return false;
    }

    public void TakeHeal(EnemySkill playerSkill, Player player)
    {
        float modifiers = Random.Range(0.85f, 1.0f);
        float a = (2 * player.Level + 10) / 250f;
        float d = a * playerSkill.skillBase.Power * ((float)player.equipEnemy.MagicPower) + 2;
        // 与えるダメージの2.5倍
        int healHP = Mathf.FloorToInt(d * modifiers * 2.5f);

        Hp += healHP;
        if (Hp > MaxHp)
        {
            Hp = MaxHp;
        }
    }

    public void UseSp(EnemySkill playerSkill)
    {
        Sp -= playerSkill.skillBase.Sp;
    }

    // expを更新して上がった経験値分のnextExpを返す
    public ExpPair GetExp(int getExp)
    {
        ExpPair expPair;
        expPair.oldLevel = Level;
        List<float> currentExps = new List<float>();
        List<float> nextExps = new List<float>();
        nextExp = nextExp - Exp;// 次の経験値までの経験値
        Exp += getExp;
        nextExps.Add(nextExp);
        int remainExp = nextExp - getExp;// 次のレベルまでのexp
        float remainGetExp = getExp;
        while (remainExp <= 0)// 次のレベルまでのexpが－だったら
        {
            // レベルアップ
            Level += 1;
            remainGetExp -= nextExp;
            currentExps.Add(nextExp);
            // 次までの経験値の更新
            nextExp = expSheet.sheets[0].list[Level - 1].nextExp;
            nextExps.Add(nextExp);
            int deltaExp = Mathf.Abs(Exp - nextExp);// 余った経験値
            remainExp = nextExp - deltaExp;
        }
        Exp = (int)remainGetExp;
        currentExps.Add(remainGetExp);
        expPair.getExp = currentExps;
        expPair.nextExp = nextExps;
        expPair.newLevel = Level;

        return expPair;
    }
}

public struct ExpPair
{
    public int oldLevel;
    public int newLevel;
    public List<float> getExp;
    public List<float> nextExp;
}