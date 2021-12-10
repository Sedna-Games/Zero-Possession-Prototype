using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    [SerializeField] float sightRadius = 30.0f;
    [Header("References")]
    [SerializeField] WeaponManager weaponManager = null;

    GameObject player = null;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("PlayerCapsule");
    }

    // Update is called once per frame
    void Update()
    {

        if ((player.transform.position - transform.position).magnitude <= sightRadius)
        {
            transform.LookAt(player.transform);
            weaponManager.OnPrimaryWeapon();
        }

    }
}
