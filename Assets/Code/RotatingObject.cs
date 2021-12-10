using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingObject : MonoBehaviour
{
    [SerializeField] Vector3 rotation = Vector3.zero;
    [SerializeField] float lerpSpeed = 1.0f;
    private Vector3 _orignalPosition;
    [SerializeField] Transform lerpPoint;
    [SerializeField] AnimationCurve lerpCurve;

    private void Start()
    {
        _orignalPosition = transform.localPosition;
        IEnumerator Lerp()
        {
            while (true)
            {
                yield return null;
                float x = 0.0f;
                while (x < 1.0f)
                {
                    yield return new WaitForEndOfFrame();
                    x += Time.deltaTime * lerpSpeed;
                    transform.localPosition = Vector3.Lerp(_orignalPosition, lerpPoint.localPosition, lerpCurve.Evaluate(x));
                }
                x = 1.0f;
                while (x > 0.0f)
                {
                    yield return new WaitForEndOfFrame();
                    x -= Time.deltaTime * lerpSpeed;
                    transform.localPosition = Vector3.Lerp(_orignalPosition, lerpPoint.localPosition, lerpCurve.Evaluate(x));
                }
                x = 0.0f;
            }
        }
        StartCoroutine(Lerp());
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rotation);
    }
}
