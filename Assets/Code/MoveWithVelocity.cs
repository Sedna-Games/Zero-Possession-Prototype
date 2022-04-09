using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class MoveWithVelocity : MonoBehaviour
{
    [SerializeField] float movementScalar = 0.5f;
    [SerializeField] float mouseDecay = 1.5f;

    [Header("References")]
    [SerializeField] new Rigidbody rigidbody = null;
    [SerializeField] GameObject pCamera = null;

    Vector3 _mouseVec = Vector3.zero;
    Vector3 _originalPos = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        _originalPos = transform.localPosition;
        IEnumerator DecayMouseVec()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                _mouseVec /= mouseDecay;
            }
        }
        StartCoroutine(DecayMouseVec());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var dir = pCamera.transform.InverseTransformDirection(rigidbody.velocity) * movementScalar;
        dir.y = -dir.y;
        dir.x = -dir.x;
        dir += _mouseVec;
        var vel = new Vector3(dir.x, dir.y);

        transform.localPosition = _originalPos + vel * Time.fixedDeltaTime;
        transform.localPosition -= new Vector3(0.0f, 0.0f, transform.localPosition.z);
    }

    public void OnLook(InputValue value)
    {
        _mouseVec = value.Get<Vector2>();
        _mouseVec.x = -_mouseVec.x;
    }
}
