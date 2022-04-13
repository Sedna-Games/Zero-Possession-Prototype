using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DamageDoer : MonoBehaviour
{
    public string ddName = "";
    public bool overrideAllEdgeCases = false;
    public float damage = 1.0f;
    public UnityEvent OnDidDamage;
    public UnityEvent OnFailedDoDamage;
    public bool canDoDamage = true;
    [SerializeField] bool continuousDamageDoer = true;
    public List<string> ignoreTags = new List<string>();

    protected bool _overrideIgnoreTags = false;

    private void OnEnable()
    {
        _overrideIgnoreTags = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (continuousDamageDoer)
            return;
        BeginDoDamage(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!continuousDamageDoer)
            return;
        BeginDoDamage(other);
    }

    protected virtual void BeginDoDamage(Collider other)
    {
        if (overrideAllEdgeCases)
        {
            DoDamage(other.gameObject);
            return;
        }

        if (CheckEdgeCases(other))
        {
            OnFailedDoDamage.Invoke();
            canDoDamage = false;
            return;
        }
        if (!canDoDamage || (ignoreTags.Contains(other.tag) && !_overrideIgnoreTags))
            return;

        DoDamage(other.gameObject);
    }

    public void DoDamage(GameObject other)
    {
        var hp = other.GetComponent<Health>();
        if (hp == null)
            return;

        hp.TakeDamage(damage);
        OnDidDamage.Invoke();
    }

    protected bool CheckEdgeCases(Collider other)
    {
        bool failed =
        !other.isTrigger
        &&
        OnFailedDoDamage.GetPersistentEventCount() > 0
        &&
        canDoDamage
        &&
        !other.CompareTag("Player")
        &&
        !other.CompareTag("Enemy");

        var dd = other.GetComponent<DamageDoer>();

        failed |= dd && dd.canDoDamage && !dd.overrideAllEdgeCases;

        return failed;
    }

    public void OverrideIgnoreTags()
    {
        _overrideIgnoreTags = true;
    }

}
