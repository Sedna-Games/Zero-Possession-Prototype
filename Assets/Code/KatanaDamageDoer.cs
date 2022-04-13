using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KatanaDamageDoer : DamageDoer
{
    [Header("References")]
    [SerializeField] Rigidbody playerRigidbody = null;
    protected override void BeginDoDamage(Collider other)
    {
        Debug.Log(other.gameObject.name);
        if (CheckEdgeCases(other))
        {
            OnFailedDoDamage.Invoke();
            canDoDamage = false;
            return;
        }
        if (!canDoDamage || (ignoreTags.Contains(other.tag) && !_overrideIgnoreTags))
            return;

        var dd = other.GetComponent<DamageDoer>();

        if (!dd)
            DoDamage(other.gameObject);
        else if (dd.ddName == "EBullet")
        {
            var ddRb = dd.transform.parent.GetComponent<Rigidbody>();
            var mag = ddRb.velocity.magnitude;
            ddRb.velocity = playerRigidbody.transform.forward * mag;

            dd.OverrideIgnoreTags();
            dd.canDoDamage = true;
        }

    }
}
