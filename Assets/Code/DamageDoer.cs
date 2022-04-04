using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DamageDoer : MonoBehaviour
{
    public float damage = 1.0f;
    public UnityEvent OnDidDamage;
    public bool canDoDamage = true;
    public List<string> ignoreTags = new List<string>();

    private void OnTriggerStay(Collider other)
    {
        DoDamage(other.gameObject);
    }

    public void DoDamage(GameObject other)
    {
        if (!canDoDamage || ignoreTags.Contains(other.tag))
            return;
        var hp = other.GetComponent<Health>();
        if (hp == null)
            return;
        hp.TakeDamage(damage);
        OnDidDamage.Invoke();
    }

}
