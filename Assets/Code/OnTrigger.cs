using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnTrigger : MonoBehaviour
{
    [SerializeField] UnityEvent OnTriggerEvent;

    private void OnTriggerEnter(Collider other)
    {
        OnTriggerEvent.Invoke();
    }
}
