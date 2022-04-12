using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Katana : Weapon
{
    [SerializeField] int numPossibleSwings = 3;
    [Header("References")]
    [SerializeField] Animator animator = null;
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
        animator.SetTrigger("Attack");
        var index = Random.Range(0, numPossibleSwings + 1);
        if (index == _lastIndex)
            index = (index + 1) % numPossibleSwings;
        _lastIndex = index;
        animator.SetInteger("SwipeIndex", index);

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
