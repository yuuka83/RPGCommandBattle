using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySkill
{
	public SkillBase skillBase { get; set; }
	public int Sp { get; set; }

	public EnemySkill(SkillBase sBase)
	{
		skillBase = sBase;
		Sp = sBase.Sp;
	}
}
