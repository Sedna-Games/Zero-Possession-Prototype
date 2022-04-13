using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] List<GameObject> enemies;

    public void resetEnemies() {
        foreach (GameObject obj in enemies)
            obj.SetActive(true);
    }
}
