using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] Transform respawnPoint;
    [SerializeField] UnityEvent OnReachCheckpoint = null;
    bool _firstTime = false;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            if (!_firstTime)
                OnReachCheckpoint.Invoke();
            _firstTime = true;
            var resetter = other.GetComponent<DeathReset>();
            if (resetter ==null)
                return;
            resetter.setResetPoint(respawnPoint);

        }
    }
}
