using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class World
{
    // ゲームシーン
    public enum GameMode
    {
        FIELD_SCENE,
        BATTLE_SCENE,
        RESULT_SCENE,
        BOSS_SCENE
    }

    //属性
    public enum MagicType
    {
        NOTHING,
        FIRE,
        ICE,
        WIND,
        THUNDER,
        DARK,
        HOLY,
        END
    }
}