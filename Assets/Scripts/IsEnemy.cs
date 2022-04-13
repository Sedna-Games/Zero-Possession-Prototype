using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsEnemy : MonoBehaviour
{
    Vector3 _originalPos = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        _originalPos = transform.position;
        EnemyManager._instance.AddEnemyToList(this);
    }

    public void ResetEnemy()
    {
        transform.position = _originalPos;
        gameObject.SetActive(true);
    }

}
