using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class SkillBase : ScriptableObject
{
    // スキルのマスターデータ

    [SerializeField] private string skillName;

    [TextArea]
    [SerializeField] private string description;

    [SerializeField] private World.MagicType magicType;
    [SerializeField] private int power;
    [SerializeField] private int sp;
    [SerializeField] private int accuracy;// 命中率
    [SerializeField] private bool isAll;// 単体攻撃か
    [SerializeField] private bool isAttackSkill;// 攻撃魔法か
    [SerializeField] private bool isHeal;// 回復魔法か
    [SerializeField] private bool isRevival;// 復活魔法か
    [SerializeField] private bool isEffective;// 復活魔法か

    [SerializeField] private GameObject skillRecieveEffect;

    public string SkillName { get => skillName; }
    public string Description { get => description; }
    public World.MagicType MagicType { get => magicType; }
    public int Power { get => power; }
    public int Sp { get => sp; }
    public int Accuracy { get => accuracy; }
    public bool IsAll { get => isAll; }
    public GameObject SkillRecieveEffect { get => skillRecieveEffect; }
    public bool IsHeal { get => isHeal; }
    public bool IsRevival { get => isRevival; }
    public bool IsAttackSkill { get => isAttackSkill; }
}