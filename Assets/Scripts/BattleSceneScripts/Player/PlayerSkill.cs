using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkill
{
	public SkillBase skillBase { get; set; }
	public int Sp { get; set; }

	public PlayerSkill(SkillBase sBase)
	{
		skillBase = sBase;
		Sp = sBase.Sp;
	}
}
