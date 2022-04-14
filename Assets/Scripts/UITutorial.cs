using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UITutorial : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tmpUI;
    [SerializeField] private string displayText;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            tmpUI.text = displayText;
            tmpUI.gameObject.SetActive(true);

        }

    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.tag == "Player")
        {
            tmpUI.gameObject.SetActive(false);
            tmpUI.text = "";
        }

    }
}
