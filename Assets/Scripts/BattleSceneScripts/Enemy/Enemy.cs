using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Enemy : Character
{
    [SerializeField] private EnemyBase enemyBase;
    [SerializeField] private int level;

    // ベースとなるデータ
    public EnemyBase EnemyBase { get => enemyBase; }

    public int Level { get => level; }
    public GameObject EnemyModel { get; set; }
    public Animator EnemyAnimator { get; set; }

    public BattleEnemyUI EnemyUI { get; set; }

    // 使える技
    public List<EnemySkill> Skills { get; set; }

    [SerializeField] private int physicPower;// 力
    [SerializeField] private int magicPower;// 魔
    [SerializeField] private int def;// 耐
    [SerializeField] private int agi;// 速
    [SerializeField] private int luck; // 運

    public int Hp { get; set; }

    // コンストラクタ:生成時の初期設定
    public Enemy(EnemyBase eBase, int eLevel)
    {
        //しょきか
        isPlayer = false;
        enemyBase = eBase;
        level = eLevel;
        Hp = MaxHp;
        //		agi = eBase.Agi;
        Skills = new List<EnemySkill>();
        // 覚える技のレベル以上ならslillsに追加
        foreach (LearnableSkill learableSkill in eBase.LearableEnemySkills)
        {
            if (Level >= learableSkill.Level)
            {
                Skills.Add(new EnemySkill(learableSkill.SkillBase));
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
        get { return Mathf.FloorToInt((EnemyBase.MaxHp * Level) / 100f) + 10; }
    }

    public int PhysicPower
    { get { return Mathf.FloorToInt((EnemyBase.PhysicPower * Level) / 100f) + 5; } }

    public int MagicPower
    { get { return Mathf.FloorToInt((EnemyBase.MagicPower * Level) / 100f) + 5; } }

    public int Def
    { get { return Mathf.FloorToInt((EnemyBase.Def * Level) / 100f) + 5; } }

    public int Agi
    { get { return Mathf.FloorToInt((EnemyBase.Agi * Level) / 100f) + 5; } }

    public int Luck
    { get { return Mathf.FloorToInt((EnemyBase.Luck * Level) / 100f) + 5; } }

    public bool isEffective(EnemySkill playerSkill)
    {
        for (int i = 0; i < this.EnemyBase.WeakTypes.Length; i++)
        {
            if (playerSkill.skillBase.MagicType == this.EnemyBase.WeakTypes[i])
            {
                return true;
            }
        }
        return false;
    }

    public bool isResist(EnemySkill playerSkill)
    {
        for (int i = 0; i < this.EnemyBase.ResistanceTypes.Length; i++)
        {
            // 使われたスキルが耐性だったら
            if (playerSkill.skillBase.MagicType == this.EnemyBase.ResistanceTypes[i])
            {
                return true;
            }
        }
        return false;
    }

    public bool isInvalid(EnemySkill playerSkill)
    {
        for (int i = 0; i < this.EnemyBase.InvalidTypes.Length; i++)
        {
            if (playerSkill.skillBase.MagicType == this.EnemyBase.InvalidTypes[i])
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

    public bool TakeDamage(EnemySkill playerSkill, Player player)
    {   // クリティカル
        // 相性
        float effectiveness = 1;// 効果量
        if (isEffective(playerSkill))
        {
            effectiveness = 1.5f;
        }
        else if (isResist(playerSkill))
        {
            effectiveness = 0.5f;
        }
        else if (isInvalid(playerSkill))
        {
            effectiveness = 0;
        }

        float modifiers = Random.Range(0.85f, 1.0f) * effectiveness * critical;
        float a = (2 * player.Level + 10) / 250f;
        float d = a * playerSkill.skillBase.Power * ((float)player.EquipEnemy.MagicPower / Def) + 2;
        int damage = Mathf.FloorToInt(d * modifiers);

        Hp -= damage;
        if (Hp <= 0)
        {
            Hp = 0;
            return true;
        }
        return false;
    }

    public void TakeHeal(EnemySkill enemSkill)
    {
        float modifiers = Random.Range(0.85f, 1.0f);
        float a = (2 * Level + 10) / 250f;
        float d = a * enemSkill.skillBase.Power * (MagicPower) + 2;
        int healHP = Mathf.FloorToInt(d * modifiers * 2.5f);

        Hp += healHP;
        if (Hp > MaxHp)
        {
            Hp = MaxHp;
        }
    }
}