using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonaParty : MonoBehaviour
{
    [SerializeField] private List<Enemy> enemyList;

    public List<Enemy> EnemyList { get => enemyList; }
}