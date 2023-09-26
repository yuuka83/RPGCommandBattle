using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[CreateAssetMenu]
public class PlayerBase : ScriptableObject
{
    [SerializeField]
    private int playerId;

    [SerializeField]
    private string playerName;

    [SerializeField]
    private GameObject playerModel;

    //
    [SerializeField]
    private World.MagicType[] weakTypes;

    [SerializeField]
    private World.MagicType[] resistanceTypes;

    [SerializeField] private World.MagicType[] invalidTypes;

    [SerializeField]
    private int playerMaxHp;//HP最大値

    [SerializeField]
    private int playerMaxSp;//SP最大値

    [SerializeField]
    private int playerPhysicAtk;//物理攻撃力

    [SerializeField]
    private int playerPhysicDef;// 物理防御力

    [SerializeField]
    private int playerMagicAtk;//魔法攻撃力

    [SerializeField]
    private int playerMagicDef;// 魔法防御力

    [SerializeField]
    private int playerAgi;

    [SerializeField]
    private int playerLv;

    // 所持できるペルソナ
    [SerializeField] private List<Enemy> EnemyList { get; set; }// 所持ペルソナ

    // 覚える技一覧
    [SerializeField] private List<LearnableSkill> learnablePlayerSkills;

    //プロパティ
    public int PlayerId
    {
        get { return playerId; }
    }

    public int PlayerMaxHp
    {
        get { return playerMaxHp; }
    }

    public int PlayerMaxSp
    {
        get { return playerMaxSp; }
    }

    public int PlayerPhysicAtk
    {
        get { return playerPhysicAtk; }
    }

    public int PlayerPhysicDef
    {
        get { return playerPhysicDef; }
    }

    public int PlayerAgi
    {
        get { return playerAgi; }
    }

    public int PlayerLv
    {
        get { return playerLv; }
    }

    public string PlayerName { get => playerName; }
    public List<LearnableSkill> LearnablePlayerSkills { get => learnablePlayerSkills; }
    public GameObject PlayerModel { get => playerModel; }
    public World.MagicType[] WeakTypes { get => weakTypes; }
    public World.MagicType[] ResistanceTypes { get => resistanceTypes; }
    public World.MagicType[] InvalidTypes { get => invalidTypes; }
    public int PlayerMagicAtk { get => playerMagicAtk; }
    public int PlayerMagicDef { get => playerMagicDef; }

    //弱点を返す関数
    public World.MagicType[] GetPlayerWeakPoint()
    {
        return WeakTypes;
    }

    public World.MagicType[] GetPlayerStrongPoint()
    {
        return ResistanceTypes;
    }
}