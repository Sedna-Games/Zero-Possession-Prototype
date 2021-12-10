using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] Transform respawnPoint;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            var resetter = other.GetComponent<DeathReset>();
            if (resetter ==null)
                return;
            resetter.setResetPoint(respawnPoint);
        }
    }
}
