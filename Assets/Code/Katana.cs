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
        var index = Random.Range(0,numPossibleSwings);
        if(index == _lastIndex)
            index = (index + 1) % numPossibleSwings;
        _lastIndex = index;
        animator.SetInteger("SwipeIndex",index);
        // IEnumerator Lerp()
        // {
        //     _attacking = true;
        //     damageDoer.canDoDamage = true;
        //     var lerpPos = lerpPositions[Random.Range(0,lerpPositions.Count)];
        //     float x = 0.0f;
        //     while (x < 1.0f)
        //     {
        //         yield return new WaitForEndOfFrame();
        //         x += Time.deltaTime * lerpSpeed;
        //         transform.localPosition = Vector3.Slerp(orignalPos, lerpPos.localPosition, movementCurve.Evaluate(x));
        //         transform.localRotation = Quaternion.Slerp(orignalRotation, lerpPos.localRotation, rotationCurve.Evaluate(x));
        //     }

        //     x = 1.0f;
        //     while (x > 0.0f)
        //     {
        //         yield return new WaitForEndOfFrame();
        //         x -= Time.deltaTime * lerpSpeed;
        //         transform.localPosition = Vector3.Slerp(orignalPos, lerpPos.localPosition, movementCurve.Evaluate(x));
        //         transform.localRotation = Quaternion.Slerp(orignalRotation, lerpPos.localRotation, rotationCurve.Evaluate(x));
        //     }
        //     x = 0.0f;
        //     damageDoer.canDoDamage = false;
        //     _attacking = false;
        // }
        // StartCoroutine(Lerp());
    }

    public void SetAttacking(bool yn){
            _attacking = yn;
            damageDoer.canDoDamage = yn;
        switch(yn){
            case true:
            particleSystems.ForEach((x) => x.Play());
            break;
            case false:
            particleSystems.ForEach((x) => x.Stop());
            break;
        }
    }
}
