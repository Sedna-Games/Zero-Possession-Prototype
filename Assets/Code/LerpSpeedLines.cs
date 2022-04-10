using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpSpeedLines : MonoBehaviour
{
    [SerializeField] float alphaMax = 0.75f;
    [SerializeField] AnimationCurve curve = null;
    [SerializeField] float maxSpeed = 34.0f;
    [Header("References")]
    [SerializeField] ParticleSystem speedLines = null;
    [SerializeField] new Rigidbody rigidbody = null;
    float tValue = 0.0f;
    
    ParticleSystem.MainModule main;
    Color color;
    // Start is called before the first frame update
    void Start()
    {
        main = speedLines.main;
        color = main.startColor.color;
    }

    // Update is called once per frame
    void Update()
    {
        tValue = rigidbody.velocity.magnitude / maxSpeed;
        tValue = Mathf.Clamp(tValue,0.0f,1.0f);
        main.startColor = Color.Lerp(new Color(color.r,color.g,color.b,0.0f),new Color(color.r,color.g,color.b,alphaMax),curve.Evaluate(tValue));
    }
}
