using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Timer : MonoBehaviour {
    [SerializeField] TextMeshProUGUI textMesh;
    TimeSpan time;
    TimeSpan timePart2;
    static float timer = 0f;
    static float actualTimeLol = 0.0f;
    static bool isTimerStopped = false;

    private void Update() {
        if (!isTimerStopped) {
            timer += Time.deltaTime;
            actualTimeLol = timer;
            time = TimeSpan.FromSeconds(timer);
            textMesh.text = String.Format("{0:00}:{1:00.00}", Math.Floor(time.TotalMinutes), time.TotalSeconds%60);
        }
        else {
            timePart2 = TimeSpan.FromSeconds(actualTimeLol);
            textMesh.text = String.Format("{0:00}:{1:00.00}", Math.Floor(timePart2.TotalMinutes), timePart2.TotalSeconds%60);
        }
    }
    public void ResetTimer() {
        timer = 0.0f;
        isTimerStopped = false;
        
        // heheheheheheheheeheheheh
    }

    public void StopTimer() {
        isTimerStopped = true;
    }
}
