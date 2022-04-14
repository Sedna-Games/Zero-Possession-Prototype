using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DeathCounter : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textmesh;
    int _deathCount = 0;

    
    // Start is called before the first frame update
    void Start()
    {
        _deathCount = 0;

        string _deathsText = "Deaths: ";
        textmesh.text = _deathsText + _deathCount.ToString();
    }

    public void IncrementDeathCount() {
        _deathCount++;

        string _deathsText = "Deaths: ";
        textmesh.text = _deathsText + _deathCount.ToString();
    }
}