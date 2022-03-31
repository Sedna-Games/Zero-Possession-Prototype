using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveWithVelocity : MonoBehaviour
{
    [SerializeField] float movementScalar = 0.5f;

    [Header("References")]
    [SerializeField] new Rigidbody rigidbody = null;
    [SerializeField] GameObject pCamera = null;

    Vector3 _originalPos = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        _originalPos = transform.localPosition;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var dir = pCamera.transform.InverseTransformDirection(rigidbody.velocity) * movementScalar;
        var vel = new Vector3(dir.x, dir.y);

        transform.localPosition = _originalPos + vel*Time.fixedDeltaTime;
        transform.localPosition -= new Vector3(0.0f, 0.0f, transform.localPosition.z);
    }
}
