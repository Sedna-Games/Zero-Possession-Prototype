using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class MoveWithVelocity : MonoBehaviour
{
    [System.Serializable]
    public enum UpdateTime
    {
        Update = 0,
        FixedUpdate,
        LateUpdate,
    }
    [SerializeField] UpdateTime updateTime = UpdateTime.Update;
    [SerializeField] float smoothing = 10.0f;
    [SerializeField] float movementScalar = 1.0f;
    [SerializeField] float mouseScalar = 1.0f;

    [Header("References")]
    [SerializeField] PlayerController playerController = null;
    [SerializeField] new Rigidbody rigidbody = null;
    [SerializeField] GameObject pCamera = null;

    Vector3 _moveVec = Vector3.zero;
    Vector3 _mouseVec = Vector3.zero;
    Vector3 _originalPos = Vector3.zero;
    void Start()
    {
        _originalPos = transform.localPosition;
    }

    void Update()
    {
        if (updateTime == UpdateTime.Update)
            Move(Time.deltaTime);
    }

    void FixedUpdate()
    {
        if (updateTime == UpdateTime.FixedUpdate)
            Move(Time.fixedDeltaTime);
    }

    private void LateUpdate()
    {
        if (updateTime == UpdateTime.LateUpdate)
            Move(Time.smoothDeltaTime);
    }

    void Move(float dt)
    {
        var dir = pCamera.transform.InverseTransformDirection(rigidbody.velocity);
        dir.x = -dir.x;
        dir.y = -dir.y;
        _moveVec += dir;

        var vel = _moveVec * movementScalar + _mouseVec * mouseScalar;
        vel = _originalPos + vel * dt;

        transform.localPosition = vel;
        transform.localPosition -= new Vector3(0.0f, 0.0f, transform.localPosition.z);

        _moveVec -= _moveVec * Time.fixedDeltaTime * smoothing;
        _mouseVec -= _mouseVec * Time.deltaTime * smoothing;
    }

    public void OnLook(InputValue value)
    {
        var temp = (Vector3)value.Get<Vector2>()*playerController.RotationSpeed;
        temp.x = -temp.x;
        _mouseVec += temp;
    }
}
