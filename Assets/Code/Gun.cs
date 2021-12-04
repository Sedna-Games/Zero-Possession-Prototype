using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : Weapon
{
    [SerializeField] List<Transform> bulletEmitterPoints = new List<Transform>();
    public override void Attack()
    {
        base.Attack();
    }
}
