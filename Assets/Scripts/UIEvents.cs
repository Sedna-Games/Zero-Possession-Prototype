using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIEvents : MonoBehaviour
{
    [SerializeField] PlayerController player;
    [SerializeField] Image cursorImage;
    [SerializeField] Color cursorLungeColor = Color.red;
    [SerializeField] Image dashImage;

    void Update() {
        if (player.inLungeRange)
            cursorImage.color = cursorLungeColor;
        else
            cursorImage.color = Color.white;
        dashImage.fillAmount = player.dashFill;
    }
}
