using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DeathReset : MonoBehaviour
{
    [SerializeField] UnityEvent OnPostDeath = null;
    [Header("References")]
    [SerializeField] Transform resetPoint = null;

    public void Die()
    {
        transform.localPosition = resetPoint.localPosition;
        OnPostDeath.Invoke();
    }
}
