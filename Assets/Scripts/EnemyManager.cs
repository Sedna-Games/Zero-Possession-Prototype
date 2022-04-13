using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{

    public static EnemyManager _instance = null;

    private void Awake()
    {
        if (_instance != null)
        {
            Debug.LogError("Enemy Manager has duplicate instance! Disabling " + gameObject.name);
            gameObject.SetActive(false);
        }
        _instance = this;
    }

    private void OnDestroy()
    {
        _instance = null;
    }

    List<IsEnemy> _enemies = new List<IsEnemy>();
    public void resetEnemies()
    {
        foreach (var item in _enemies)
            item.ResetEnemy();
    }
    public void AddEnemyToList(IsEnemy enemy)
    {
        _enemies.Add(enemy);
    }
}
