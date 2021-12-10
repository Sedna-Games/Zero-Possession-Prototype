using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpSpeedLines : MonoBehaviour
{
    [SerializeField] float alphaMax = 0.75f;
    [SerializeField] ParticleSystem speedLines = null;

    [Range(0.0f,1.0f)]
    public float tValue = 0.0f;
    
    ParticleSystem.MainModule main;
    // Start is called before the first frame update
    void Start()
    {
        main = speedLines.main;
    }

    // Update is called once per frame
    void Update()
    {
        main.startColor = Color.Lerp(new Color(1.0f,1.0f,1.0f,0.0f),new Color(1.0f,1.0f,1.0f,alphaMax),tValue);
    }
}
