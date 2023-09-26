using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class EnemyBase : ScriptableObject
{
    [SerializeField] private string enemyName;

    [TextArea]
    [SerializeField] private string description;

    // モデル
    [SerializeField] private GameObject enemyModel;

    // 弱点
    [SerializeField] private World.MagicType[] weakTypes;

    // 耐性
    [SerializeField] private World.MagicType[] resistanceTypes;

    // 無効
    [SerializeField] private World.MagicType[] invalidTypes;

    // ステータス
    [SerializeField] private int maxHp;

    [SerializeField] private int physicPower;// 力
    [SerializeField] private int magicPower;// 魔
    [SerializeField] private int def;// 耐
    [SerializeField] private int agi;// 速
    [SerializeField] private int luck; // 運

    // 覚える技一覧
    [SerializeField] private List<LearnableSkill> learnableEnemySkills;

    public int MaxHp { get => maxHp; }
    public int Agi { get => agi; }
    public int PhysicPower { get => physicPower; }
    public int Def { get => def; }
    public int MagicPower { get => magicPower; }
    public int Luck { get => luck; }
    public World.MagicType[] WeakTypes { get => weakTypes; }
    public World.MagicType[] ResistanceTypes { get => resistanceTypes; }
    public World.MagicType[] InvalidTypes { get => invalidTypes; }
    public List<LearnableSkill> LearableEnemySkills { get => learnableEnemySkills; }
    public string EnemyName { get => enemyName; }
    public string Description { get => description; }
    public GameObject EnemyModel { get => enemyModel; }
}

// 覚える技：どのレベルで何を覚えるのか
[Serializable]
public class LearnableSkill
{
    [SerializeField] private SkillBase skillBase;
    [SerializeField] private int level;

    public SkillBase SkillBase { get => skillBase; }
    public int Level { get => level; }
}