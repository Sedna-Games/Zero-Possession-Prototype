using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UISpeed : MonoBehaviour
{

    [SerializeField] private Rigidbody rigidBody;
    [SerializeField] private TextMeshProUGUI tmpUI;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        tmpUI.text = rigidBody.velocity.magnitude.ToString("#") + " m/s";
    }
}
