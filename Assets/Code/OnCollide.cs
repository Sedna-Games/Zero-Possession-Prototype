using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnCollide : MonoBehaviour
{
    [SerializeField] UnityEvent OnCollideEvent;

    private void OnCollisionEnter(Collision other)
    {
        OnCollideEvent.Invoke();
    }
}
