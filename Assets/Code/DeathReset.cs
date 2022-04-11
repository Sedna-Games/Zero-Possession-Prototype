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
        transform.position = resetPoint.position;
        transform.localRotation = resetPoint.rotation;
        OnPostDeath.Invoke();
    }

    public void setResetPoint(Transform point) {
        resetPoint = point;
    }
}
