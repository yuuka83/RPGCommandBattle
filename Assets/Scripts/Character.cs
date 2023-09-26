using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Character
{
    public bool isPlayer;

    //public int agi;
    public bool isDying;// 瀕死状態か

    //public bool isFaintedPreviousTurn;//前のターンの気絶状態
    public bool isFainted;//気絶状態か
}