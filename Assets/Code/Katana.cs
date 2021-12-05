using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Katana : Weapon
{
    Vector3 orignalPos = Vector3.zero;
    Quaternion orignalRotation = Quaternion.identity;

    [SerializeField] AnimationCurve movementCurve = null;
    [SerializeField] AnimationCurve rotationCurve = null;
    [SerializeField] float lerpSpeed = 1.0f;

    [Header("References")]
    [SerializeField] List<Transform> lerpPositions = null;
    [SerializeField] DamageDoer damageDoer = null;
    // Start is called before the first frame update
    void Start()
    {
        orignalPos = transform.localPosition;
        orignalRotation = transform.localRotation;
        damageDoer.canDoDamage = false;
    }

    public override void Attack()
    {
        base.Attack();
        IEnumerator Lerp()
        {
            _attacking = true;
            damageDoer.canDoDamage = true;
            var lerpPos = lerpPositions[Random.Range(0,lerpPositions.Count)];
            float x = 0.0f;
            while (x < 1.0f)
            {
                yield return new WaitForEndOfFrame();
                x += Time.deltaTime * lerpSpeed;
                transform.localPosition = Vector3.Slerp(orignalPos, lerpPos.localPosition, movementCurve.Evaluate(x));
                transform.localRotation = Quaternion.Slerp(orignalRotation, lerpPos.localRotation, rotationCurve.Evaluate(x));
            }

            x = 1.0f;
            while (x > 0.0f)
            {
                yield return new WaitForEndOfFrame();
                x -= Time.deltaTime * lerpSpeed;
                transform.localPosition = Vector3.Slerp(orignalPos, lerpPos.localPosition, movementCurve.Evaluate(x));
                transform.localRotation = Quaternion.Slerp(orignalRotation, lerpPos.localRotation, rotationCurve.Evaluate(x));
            }
            x = 0.0f;
            damageDoer.canDoDamage = false;
            _attacking = false;
        }
        StartCoroutine(Lerp());
    }
}
