using System.Collections;
using System.Collections.Generic;
using InputManagerScript;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    [Header("Movement Settings"), Space(10)]
    [SerializeField] float moveSpeed = 10.0f;
    [SerializeField] float _dashSpeed = 25.0f;
    [SerializeField] float maxSpeed = 40.0f;
    [SerializeField] float maxSpeedChange = 10.0f;
    [SerializeField] float _slideSpeed = 17.5f;
    [SerializeField, Tooltip("The speed while sliding lerps from slideSpeed to slowSlideSpeed")]
    float _slowSlideSpeed = 17.5f;
    [SerializeField, Tooltip("The amount of seconds to go from fast slide speed to slow slide speed")]
    float slideSlowRate = 5f;
    [SerializeField] float _climbSpeed = 10.0f;
    [SerializeField] float wallRunTiltAngle = 15.0f, lateralWallRunTiltAngle = -50.0f, slideTiltAngle = -35.0f, tiltSpeed = 2.5f;
    [SerializeField] int maxAirJumps = 1, maxDashes = 1;
    [SerializeField] float _jumpHeight = 2.0f;
    [SerializeField, Tooltip("Extra jump multiplier from walls")]
    float wallJumpMultiplier = 5f;
    [SerializeField, Range(0f, 1f), Tooltip("Bunny hop-like momentum on jumps")]
    float _jumpMomentum = 0.05f;
    [SerializeField] float jumpDelay = 0.2f;
    [SerializeField] float _dashCooldown = 0.5f;
    float currentSpeed;
    float currentClimbSpeed;
    float _jumpDelay = 0f;
    bool desireJump = false, desireDash = false;
    Quaternion targetRotation;

    [Header("Terrain Settings"), Space(10)]

    [SerializeField, Tooltip("Distance for ground/wall raycasts"), Min(0f)]
    float probeDistance = 1f;
    [SerializeField, Range(0f, 90f)]
    float maxGroundAngle = 10.0f, maxSlopeAngle = 80.0f;
    [SerializeField, Range(90f, 180f)]
    float maxClimbAngle = 140.0f;
    [SerializeField] float maxLateralClimbDistance = 16f;
    [SerializeField] LayerMask groundMask = -1, climbMask = -1, slopeMask = -1;
    [SerializeField, Tooltip("How sticky the player is to the wall")]
    float wallForce = 5.0f;
    [SerializeField, Tooltip("How long before the player can stick to the wall after getting off it")]
    float wallStickDelay = 0.1f;
    float _wallStickTimer = 0f;
    float _wallstickDistance;
    float _wallstickY;
    bool firstCling = true, climbable = false;

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    [SerializeField] GameObject CinemachineCameraTarget;
    [Tooltip("How far in degrees can you move the camera up")]
    [SerializeField] float TopClamp = 90.0f;
    [Tooltip("How far in degrees can you move the camera down")]
    [SerializeField] float BottomClamp = -90.0f;
    [Tooltip("Rotation speed of the character")]
    [SerializeField] float RotationSpeed = 1.0f;
    private float _rotationVelocity;

    // cinemachine
    private float _cinemachineTargetPitch;
    private const float _threshold = 0.01f;

    [Header("Assets"), Space(10)]
    [SerializeField] Rigidbody _rb;
    [SerializeField] InputManager _input;
    [SerializeField] Transform _tiltRotater;
    [SerializeField] Collider _normalCollider;
    [SerializeField] Collider _slideCollider;
    Vector3 velocity, desiredVel;
    int jumpPhase, dashPhase;
    int stepsSinceGrounded = 0, stepsSinceJump = 0;
    float dashCooldown = 0.0f;
    bool DashCooldown => dashCooldown < 0f;
    bool Sliding => _input.slide;
    bool _sliding = false;
    float minGroundDotProduct, minClimbDotProduct, minSlopeDotProduct;
    int groundContactCount, climbContactCount, slopeContactCount;
    Vector3 groundNormal, climbNormal, slopeNormal;
    bool OnGround => groundContactCount > 0;
    bool wallContact => climbContactCount > 0 && _wallStickTimer < 0f;
    bool OnSlope => slopeContactCount > 0;
    bool Climbing => wallContact && _wallStatus != WallStatus.none && !OnGround;
    WallStatus _wallStatus;
    private void Awake() {
        OnValidate();
    }
    void OnValidate() {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        minClimbDotProduct = Mathf.Cos(maxClimbAngle * Mathf.Deg2Rad);
        minSlopeDotProduct = Mathf.Cos(maxSlopeAngle * Mathf.Deg2Rad);
        currentSpeed = moveSpeed;
        currentClimbSpeed = _climbSpeed;
    }
    private void Update() {
        desiredVel = new Vector3(_input.move.x, 0f, _input.move.y) * currentSpeed;
        dashCooldown -= Time.deltaTime;
        _wallStickTimer -= Time.deltaTime;
        _jumpDelay -= Time.deltaTime;
        //NOTE: The input manager updates in Update() instead of FixedUpdate() so this helps keep consistency for button presses
        if(_input.jump) {
            desireJump = true;
            _input.jump = false;
        }
        if(_input.dash) {
            desireDash = true;
            _input.dash = false;
        }
    }
    private void FixedUpdate() {
        UpdateState();
        AdjustVelocity();
        if(desireJump) {
            desireJump = false;
            Jump();
        }
        if(desireDash) {
            desireDash = false;
            Dash();
        }
        //NOTE: The new input manager doesn't have built-in slide-like hold interaction and thus requires constant calling as a pass-through action
        if(_input.slide) {
            Slide();
        }
        _rb.velocity = velocity;
        ClearState();
    }
    private void LateUpdate() {
        CameraRotation();
    }
    void ClearState() {
        groundContactCount = climbContactCount = slopeContactCount = 0;
        groundNormal = climbNormal = slopeNormal = Vector3.zero;
    }
    void UpdateState() {
        velocity = _rb.velocity;
        stepsSinceJump++;
        stepsSinceGrounded++;
        if(OnGround || (wallContact && _wallStatus != WallStatus.none) || OnSlope) {
            dashPhase = 0;
            if(stepsSinceJump > 1)
                jumpPhase = 0;
            stepsSinceGrounded = 0;
            if(groundContactCount > 1) {
                groundNormal.Normalize();
            }
            if(climbContactCount > 1) {
                climbNormal.Normalize();
            }
            if(slopeContactCount > 1) {
                slopeNormal.Normalize();
            }
        }
        else {
            groundNormal = Vector3.up;
        }
        checkWallRun();
        if(Climbing && _input.move.y > 0f && stepsSinceJump > 1 && climbable) {
            switch(_wallStatus) {
                case (WallStatus.none):
                    break;
                case (WallStatus.left):
                    velocity += (wallForce * -Vector3.right);
                    break;
                case (WallStatus.right):
                    velocity += (wallForce * Vector3.right);
                    break;
                case (WallStatus.front):
                    break;
            }
        }
    }
    Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal) {
        return (direction - normal * Vector3.Dot(direction, normal)).normalized;
    }
    void AdjustVelocity() {
        float currentX = 0f, currentZ = 0f;
        Vector3 xAxis, zAxis;


        if(Climbing && _input.move.y > 0f && stepsSinceJump > 1) {
            if(_wallStatus == WallStatus.front)
                velocity = Vector3.up * currentClimbSpeed;
            else {
                velocity = transform.forward * _climbSpeed;
            }
            return;
        }
        if(!wallContact) {
            if(OnSlope) {
                xAxis = ProjectDirectionOnPlane(transform.right, slopeNormal);
                zAxis = ProjectDirectionOnPlane(transform.forward, slopeNormal);
            }
            else {
                xAxis = ProjectDirectionOnPlane(transform.right, groundNormal);
                zAxis = ProjectDirectionOnPlane(transform.forward, groundNormal);
            }

            currentX = Vector3.Dot(velocity, xAxis);
            currentZ = Vector3.Dot(velocity, zAxis);
            float acceleration = maxSpeedChange * Time.fixedDeltaTime;
            float newX = Mathf.MoveTowards(currentX, desiredVel.x, acceleration);
            float newZ = Mathf.MoveTowards(currentZ, desiredVel.z, acceleration);
            velocity += transform.right * (newX - currentX) + transform.forward * (newZ - currentZ);

            Vector3 capSpeed = new Vector3(velocity.x, 0f, velocity.z);
            if (OnSlope)
                capSpeed *= 1.1f;
            velocity = capSpeed.normalized * Mathf.Min(capSpeed.magnitude, maxSpeed) + transform.up * velocity.y;
        }
    }
    void Jump() {
        Vector3 jumpDirection;
        if(_jumpDelay > 0f)
            return;
        if(OnGround) {
            jumpPhase = 0;
            jumpDirection = groundNormal;
        }
        else if(wallContact && _wallStatus != WallStatus.none) {
            jumpPhase = 0;
            jumpDirection = climbNormal;
        }
        else if(maxAirJumps > 0 && jumpPhase < maxAirJumps) {
            if(jumpPhase == 0)
                jumpPhase = 1;
            else
                jumpPhase++;
            jumpDirection = groundNormal;
        }
        else {
            _input.jump = false;
            return;
        }
        _wallStickTimer = wallStickDelay;
        stepsSinceJump = 0;
        float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * _jumpHeight);
        jumpDirection = (jumpDirection + _tiltRotater.up).normalized;
        float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
        if(alignedSpeed > 0f & _wallStatus == WallStatus.none) {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }
        Vector3 momentumBoost = _rb.velocity;
        momentumBoost.y = 0f;
        velocity = momentumBoost;
        if(Climbing)
            jumpSpeed *= wallJumpMultiplier;
        velocity += ((jumpDirection * jumpSpeed) + (momentumBoost * _jumpMomentum));
        _jumpDelay = jumpDelay;
        _sliding = false;
        _input.slide = false;
    }
    void Dash() {
        Vector3 dashDirection;
        if(DashCooldown && dashPhase < maxDashes) {
            dashDirection = _input.move.normalized;
            Vector3 xDir = dashDirection.x * transform.right;
            Vector3 zDir = dashDirection.y * transform.forward;
            dashDirection = xDir + zDir;
        }
        else {
            _input.dash = false;
            return;
        }
        if(!OnGround)
            dashPhase++;
        _rb.AddForce(dashDirection * _dashSpeed, ForceMode.Impulse);
        dashCooldown = _dashCooldown;
        _input.dash = false;
    }
    void Slide() {
        if(!_sliding && Sliding && OnGround) {
            _sliding = true;
            currentSpeed = _slideSpeed;
            _slideCollider.enabled = true;
            _normalCollider.enabled = false;
            StartCoroutine(SlideDecelerate());
        }
    }
    IEnumerator SlideDecelerate() {
        while(_input.slide) {
            currentSpeed = Mathf.Lerp(currentSpeed, _slowSlideSpeed, 1f / slideSlowRate * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
        _normalCollider.enabled = true;
        _slideCollider.enabled = false;
        currentSpeed = moveSpeed;
        _sliding = false;
    }
    private void CameraRotation() {
        // if there is an input
        if(_input.look.sqrMagnitude >= _threshold) {
            _cinemachineTargetPitch += _input.look.y * RotationSpeed * Time.deltaTime;
            _rotationVelocity = _input.look.x * RotationSpeed * Time.deltaTime;

            // clamp our pitch rotation
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Update Cinemachine camera target pitch
            CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

            // rotate the player left and right
            transform.Rotate(Vector3.up * _rotationVelocity);

        }

        // Rotates player on x/z axis based on targetRotation, which tilts the camera when wall running or sliding
        Quaternion newTarget = Quaternion.identity;
        newTarget.eulerAngles = new Vector3(targetRotation.eulerAngles.x, _tiltRotater.rotation.eulerAngles.y, targetRotation.eulerAngles.z);
        _tiltRotater.rotation = Quaternion.Lerp(_tiltRotater.rotation, newTarget, Time.deltaTime * tiltSpeed);
    }
    private static float ClampAngle(float lfAngle, float lfMin, float lfMax) {
        if(lfAngle < -360f) lfAngle += 360f;
        if(lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    void wallRunDistanceCheck(float posY) {
        if(posY - _wallstickY > 0.1f) {
            _wallstickDistance += posY - _wallstickY;
            _wallstickY = posY;
        }
        if(_wallstickDistance >= maxLateralClimbDistance)
            climbable = false;
    }
    void checkWallRun() {
        WallStatus wallStatus;
        //NOTE: Wallrun checking; special case for lateral runs as the tilt causes the initial forward raycast to miss, requiring a special down+forward raycast once attached
        if(!climbable) {
            _wallStatus = WallStatus.none;
            currentClimbSpeed = Mathf.Lerp(currentClimbSpeed, -_climbSpeed, slideSlowRate * Time.fixedDeltaTime);
        }
        if(OnGround) {
            _wallStatus = WallStatus.none;
            firstCling = true;
            currentClimbSpeed = _climbSpeed;
            _wallstickDistance = 0f;
            climbable = true;
            TiltPlayer(_wallStatus);
            return;
        }

        if(Physics.Raycast(transform.position, transform.right, probeDistance, climbMask))
            wallStatus = WallStatus.right;
        else if(Physics.Raycast(transform.position, -transform.right, probeDistance, climbMask))
            wallStatus = WallStatus.left;
        else if(Physics.Raycast(transform.position, transform.forward, probeDistance, climbMask) ||
            (wallContact && Physics.Raycast(transform.position, (transform.forward + -transform.up).normalized, 2.5f * probeDistance, climbMask)))
            wallStatus = WallStatus.front;
        else
            wallStatus = WallStatus.none;
        _wallStatus = wallStatus;
        TiltPlayer(_wallStatus);
    }
    void TiltPlayer(WallStatus status) {
        if(status == WallStatus.front)
            if(firstCling) {
                firstCling = false;
                _wallstickY = transform.position.y;
                wallRunDistanceCheck(_wallstickY);
            }
            else
                wallRunDistanceCheck(transform.position.y);

        switch(status) {
            case (WallStatus.left):
                targetRotation = Quaternion.Euler(0f, 0f, -wallRunTiltAngle);
                break;
            case (WallStatus.right):
                targetRotation = Quaternion.Euler(0f, 0f, wallRunTiltAngle);
                break;
            case (WallStatus.front):
                targetRotation = Quaternion.Euler(lateralWallRunTiltAngle, 0f, 0f);
                break;
            case (WallStatus.none):
                if(_sliding)
                    targetRotation = Quaternion.Euler(slideTiltAngle, 0f, 0f);
                else
                    targetRotation = Quaternion.identity;
                break;
        }
    }
    void OnCollisionStay(Collision other) {
        EvaluateCollision(other);
    }
    void onCollisionExit(Collision other) {
        EvaluateCollision(other);
    }

    void EvaluateCollision(Collision collision) {
        int layer = collision.gameObject.layer;
        for(int i = 0; i < collision.contactCount; i++) {
            Vector3 normal = collision.GetContact(i).normal;
            if(normal.y >= minGroundDotProduct) {
                groundContactCount++;
                groundNormal += normal;
            }
            else if(normal.y >= minSlopeDotProduct) {
                slopeContactCount++;
                slopeNormal += normal;
            }
            else {
                if(normal.y >= minClimbDotProduct && (climbMask & (1 << layer)) != 0) {
                    climbContactCount++;
                    climbNormal += normal;
                }
            }
        }
    }
    enum WallStatus {
        none = 0,
        left = 1,
        right = 2,
        front = 3,
    }
}
