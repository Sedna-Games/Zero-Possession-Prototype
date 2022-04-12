using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Katana : Weapon
{
    [Header("References")]
    [SerializeField] ArmsAnimationManager armsAnimationManager = null;
    [SerializeField] DamageDoer damageDoer = null;
    [SerializeField] List<ParticleSystem> particleSystems = new List<ParticleSystem>();
    // Start is called before the first frame update
    int _lastIndex = 0;
    void Start()
    {
        damageDoer.canDoDamage = false;
    }

    public override void Attack()
    {
        base.Attack();
        armsAnimationManager.Attack();
    }

    public void SetAttacking(bool yn)
    {
        _attacking = yn;
        damageDoer.gameObject.SetActive(yn);
        damageDoer.canDoDamage = true;
        switch (yn)
        {
            case true:
                particleSystems.ForEach((x) => x.Play());
                break;
            case false:
                particleSystems.ForEach((x) => x.Stop());
                break;
        }
    }
}
