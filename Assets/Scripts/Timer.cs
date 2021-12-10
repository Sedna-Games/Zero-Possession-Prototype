using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Timer : MonoBehaviour {
    [SerializeField] TextMeshProUGUI textMesh;
    TimeSpan time;
    float timer = 0f;

    private void Update() {
        timer += Time.deltaTime;
        time = TimeSpan.FromSeconds(timer);
        textMesh.text = String.Format("{0:00}:{1:00.00}", time.TotalMinutes, time.TotalSeconds%60);
    }
}