using System;
using System.Collections;
using System.Collections.Generic;
using InputManagerScript;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    [Header("Sounds")]
    [SerializeField] FMODUnity.StudioEventEmitter footSteps = null;
    [SerializeField] FMODUnity.StudioEventEmitter dashSound = null;
    [SerializeField] FMODUnity.StudioEventEmitter slideSound = null;
    [SerializeField] FMODUnity.StudioEventEmitter landSound = null;

    [Header("Movement Settings"), Space(10)]
    [SerializeField] float gravity = -32f;
    [SerializeField] float moveSpeed = 10.0f;
    [SerializeField] float dashSpeed = 25.0f;
    [SerializeField] float maxSpeed = 40.0f;
    [SerializeField] float slideSpeed = 17.5f;
    [SerializeField, Tooltip("The speed while sliding lerps from slideSpeed to slowSlideSpeed")]
    float slowSlideSpeed = 0f;
    [SerializeField, Tooltip("How quickly it goes from fast slide speed to slow slide speed")]
    float slideSlowRate = 5f;
    [SerializeField] float runSpeed = 10f, climbSpeed = 8f;
    [SerializeField, Tooltip("The percentage of control while in the air"), Range(0f, 1f)]
    float AirManeuverabilityRate = 1.0f;
    [SerializeField] float wallRunTiltAngle = 15.0f, lateralWallRunTiltAngle = -50.0f, slideTiltAngle = -35.0f, tiltSpeed = 2.5f;
    [SerializeField] int maxAirJumps = 1, maxDashes = 1;
    [SerializeField] float jumpHeight = 2.0f;
    [SerializeField, Tooltip("Extra jump multiplier from walls")]
    float wallJumpMultiplier = 5f;
    [SerializeField, Range(0f, 1f), Tooltip("Percentage speed increase from jumping")]
    float jumpMomentum = 0.05f;
    [SerializeField, Min(1f), Tooltip("Acceleration when sliding down slopes")]
    float slideSlopeMomentum = 1.1f;
    [Tooltip("How many slide stacks you lose per fixed updated cycle")]
    float slideMomentumStackDecay = 3f;
    [SerializeField, Range(0f, 1f), Tooltip("Percentage speed increase from dashing")]
    float dashMomentum = 0.05f;
    [SerializeField, Tooltip("Multiplier on dash momentum stacks (base: 1 => airDash = 1, groundDash = 1 * dashGroundMomentumStacks")]
    int dashGroundMomentumStacks = 3;
    [SerializeField, Tooltip("How long you continue to move at dashSpeed (locks normal xz movement)")]
    float dashDuration = 0.5f;
    [SerializeField, Tooltip("Add dashDuration to the value you want to assign this")]
    float dashCooldown = 0.5f;
    [SerializeField, Tooltip("Amount of fixedUpdate cycles to go from dashSpeed to moveSpeed")]
    float dashDecelerateRate = 50f;
    [SerializeField, Tooltip("Number of FixedUpdate cycles to lose 1 stack of jump/dash momentum while not in the air (50 cycles/second")]
    int momentumStackDecay = 10;
    [SerializeField, Tooltip("The amount of time after going off an edge that you can still be considered grounded when jumping (doesn't eat air jump)")]
    float coyoteTime = 0.3f;
    float _currentSpeed, _totalSpeed;
    float _slideMomentumStacks;
    float _dashCooldown = 0.0f;
    float _dashDuration = 0f;
    float _coyoteTimer = 0f;
    bool _desireJump = false, _desireDash = false;
    Quaternion targetRotation;

    [Header("Terrain Settings"), Space(10)]

    [SerializeField, Tooltip("Distance for ground/wall raycasts"), Min(0f)]
    float probeDistance = 1f;
    [SerializeField, Range(0f, 90f)]
    float maxGroundAngle = 10.0f, maxSlopeAngle = 80.0f;
    [SerializeField, Range(90f, 180f)]
    float maxClimbAngle = 140.0f;
    [SerializeField] float maxClimbDistance = 16f;
    [SerializeField] LayerMask groundMask = -1, climbMask = -1, runMask = -1, slopeMask = -1;
    [SerializeField, Tooltip("How sticky the player is to the wall")]
    float wallForce = 5.0f;
    [SerializeField, Tooltip("How long before the player can stick to the wall after getting off it")]
    float wallStickDelay = 0.1f;
    float _wallStickTimer = 0f;
    float _wallstickDistance, _wallstickY, _climbDistance;
    bool _firstCling = true, _climbable = false;

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
    [SerializeField] Rigidbody rb;
    [SerializeField] InputManager input;
    [SerializeField] Transform tiltRotater;
    [SerializeField] Collider normalCollider;
    [SerializeField] Collider slideCollider;
    Vector3 _velocity, _desiredVel;
    int _jumpPhase, _dashPhase;
    int _jumpMomentumStacks = 0, _dashMomentumStacks = 0;
    int _stepsSinceGrounded = 0, _stepsSinceJump = 0, _stepsSinceDash = 0;
    bool _sliding = false;
    float _minGroundDotProduct, _minClimbDotProduct, _minSlopeDotProduct;
    int _groundContactCount, _climbContactCount, _slopeContactCount;
    Vector3 _groundNormal, _climbNormal, _slopeNormal;
    bool DashCooldown => _dashCooldown < 0f;
    bool Dashing => _dashDuration > 0f;
    bool Sliding => input.slide;
    bool OnGround => _groundContactCount > 0;
    bool wallContact => _climbContactCount > 0 && _wallStickTimer < 0f;
    bool OnSlope => _slopeContactCount > 0;
    bool Climbing => wallContact && _wallStatus != WallStatus.none && !OnGround;
    bool LeftRight => _wallStatus == WallStatus.left || _wallStatus == WallStatus.right;
    bool InAir => !OnGround && !OnSlope && !Climbing;
    WallStatus _wallStatus;
    private void Awake() {
        OnValidate();
    }

    private void OnEnable() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnValidate() {
        _minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        _minClimbDotProduct = Mathf.Cos(maxClimbAngle * Mathf.Deg2Rad);
        _minSlopeDotProduct = Mathf.Cos(maxSlopeAngle * Mathf.Deg2Rad);
        _currentSpeed = moveSpeed;
    }
    //NOTE: Calculates actual speed after applying all momentum stacks. Sliding uses a different setup and is based on the const Time.fixedDeltaTime instead
    float calculateMomentumStacks(float speed) {
        return speed + (speed * jumpMomentum * _jumpMomentumStacks) + (speed * dashMomentum * _dashMomentumStacks) + (_slideMomentumStacks * Time.fixedDeltaTime);
    }
    public void resetMomentumStacks() {
        _jumpMomentumStacks = 0;
        _dashMomentumStacks = 0;
        _slideMomentumStacks = 0f;
    }
    private void Update() {
        _totalSpeed = calculateMomentumStacks(_currentSpeed);
        _climbDistance = calculateMomentumStacks(maxClimbDistance);
        _desiredVel = new Vector3(input.move.x, 0f, input.move.y).normalized * _totalSpeed;
        _dashCooldown -= Time.deltaTime;
        _dashDuration -= Time.deltaTime;
        _wallStickTimer -= Time.deltaTime;
        //NOTE: The input manager updates in Update() instead of FixedUpdate() so this helps keep consistency for button presses
        if(input.jump) {
            _desireJump = true;
            input.jump = false;
        }
        if(input.dash) {
            _desireDash = true;
            input.dash = false;
        }

        CameraRotation();
        ChangeFMODParameter();
    }

    void ChangeFMODParameter() {
        //var spd = velocity.magnitude / maxSpeed;
        //footSteps.SetParameter("player_speed",spd);
    }

    private void FixedUpdate() {
        UpdateState();
        AdjustVelocity();
        if(_desireJump) {
            _desireJump = false;
            Jump();
        }
        if(_desireDash) {
            _desireDash = false;
            Dash();
        }
        //NOTE: The new input manager doesn't have built-in slide-like hold interaction and thus requires constant calling as a pass-through action
        if(input.slide) {
            Slide();
        }
        ApplyGravity();
        rb.velocity = _velocity;
        ClearState();
    }
    private void LateUpdate() {
    }

    //NOTE: More control over gravity and removes sliding downwards on slopes
    void ApplyGravity() {
        if(OnSlope)
            _velocity += gravity * _slopeNormal * Time.fixedDeltaTime;
        else if(!OnGround)
            _velocity += gravity * _groundNormal * Time.fixedDeltaTime;
    }
    void ClearState() {
        //??
        //landSound.Play();
        _groundContactCount = _climbContactCount = _slopeContactCount = 0;
        _groundNormal = _climbNormal = _slopeNormal = Vector3.zero;
    }
    void UpdateState() {
        _velocity = rb.velocity;
        _stepsSinceJump++;
        _stepsSinceDash++;
        _stepsSinceGrounded++;
        if(OnGround || (wallContact && _wallStatus != WallStatus.none) || OnSlope) {
            _dashPhase = 0;
            _coyoteTimer = 0f;
            if(_stepsSinceJump > 1) {
                _jumpPhase = 0;
                _coyoteTimer = 0f;
            }
            //NOTE: Momentum decay. Doesn't decay for wall running but decays for ground, slope, and climbing
            if(_wallStatus != WallStatus.left && _wallStatus != WallStatus.right) {
                if(_stepsSinceJump % momentumStackDecay == 0) {
                    _jumpMomentumStacks = Math.Max(0, _jumpMomentumStacks - 1);
                }
                if(_stepsSinceDash % momentumStackDecay == 0) {
                    _dashMomentumStacks = Math.Max(0, _dashMomentumStacks - 1);
                }
            }
            _stepsSinceGrounded = 0;
            if(_groundContactCount > 1) {
                _groundNormal.Normalize();
            }
            if(_climbContactCount > 1) {
                _climbNormal.Normalize();
            }
            if(_slopeContactCount > 1) {
                _slopeNormal.Normalize();
            }
        }
        else {
            _coyoteTimer += Time.fixedDeltaTime;
            _groundNormal = Vector3.up;
        }
        checkWallRun();
    }
    Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal) {
        return (direction - normal * Vector3.Dot(direction, normal)).normalized;
    }
    void AdjustVelocity() {
        float currentX = 0f, currentZ = 0f;
        Vector3 xAxis, zAxis;
        if(Dashing) {
            _stepsSinceDash = 0;
            return;
        }
        //NOTE: Based on contact normal, returns the parallel direction for x/z axis
        if(OnSlope) {
            xAxis = ProjectDirectionOnPlane(transform.right, _slopeNormal);
            zAxis = ProjectDirectionOnPlane(transform.forward, _slopeNormal);
        }
        else {
            xAxis = ProjectDirectionOnPlane(transform.right, _groundNormal);
            zAxis = ProjectDirectionOnPlane(transform.forward, _groundNormal);
        }


        //NOTE: Makes movement less instantaneous while mid-air
        currentX = Vector3.Dot(_velocity, xAxis);
        currentZ = Vector3.Dot(_velocity, zAxis);
        float acceleration = _totalSpeed * (1f / Time.fixedDeltaTime);// * AirManeuverabilityRate * 50f;
        if(InAir)
            acceleration *= AirManeuverabilityRate;
        float newX = Mathf.MoveTowards(currentX, _desiredVel.x, acceleration * Time.fixedDeltaTime);
        float newZ = Mathf.MoveTowards(currentZ, _desiredVel.z, acceleration * Time.fixedDeltaTime);

        _velocity += transform.right * (newX - currentX) + transform.forward * (newZ - currentZ);
        if(Climbing && LeftRight)
            _velocity += (wallForce * -_climbNormal);
        else if(Climbing && (_wallStatus == WallStatus.front) && input.move.y > 0f && _climbable)
            _velocity = new Vector3(_velocity.x, _desiredVel.z, 0f);

        //NOTE: Old movement code; once testing is done remove or restore
        // else {
        //     if(Climbing && LeftRight)
        //         _velocity = transform.right * _desiredVel.x + transform.forward * _desiredVel.z + (wallForce * -_climbNormal);
        //     else if(Climbing && (_wallStatus == WallStatus.front) && input.move.y > 0f && _climbable)
        //         _velocity = transform.right * _desiredVel.x + transform.up * _desiredVel.z;
        //     else if(OnSlope) {
        //         _velocity = xAxis * _desiredVel.x + zAxis * _desiredVel.z + transform.up * _velocity.y;
        //     }
        //     else
        //         _velocity = transform.right * _desiredVel.x + transform.forward * _desiredVel.z + transform.up * _velocity.y;

        //
        // }
        // if(_sliding) {
        //     if(zAxis.z < 0f) {
        //         _velocity *= slideSlopeMomentum;
        //     }
        //     else if(zAxis.z > 0f) {
        //         _velocity /= slideSlopeMomentum;
        //     }
        // }

        Vector3 capSpeed = new Vector3(_velocity.x, 0f, _velocity.z);
        _velocity = capSpeed.normalized * Mathf.Min(capSpeed.magnitude, maxSpeed) + transform.up * Mathf.Min(_velocity.y, maxSpeed);
    }
    void Jump() {
        Vector3 jumpDirection;
        if(OnGround) {
            _jumpPhase = 0;
            jumpDirection = _groundNormal;
            _jumpMomentumStacks++;
        }
        else if(OnSlope) {
            _jumpPhase = 0;
            jumpDirection = _slopeNormal;
            _jumpMomentumStacks++;
        }
        else if(Climbing) {
            _jumpPhase = 0;
            jumpDirection = _climbNormal;
            _jumpMomentumStacks++;
        }
        else if(_coyoteTimer <= coyoteTime) {
            _jumpPhase = 0;
            jumpDirection = _groundNormal;
            _jumpMomentumStacks++;
        }
        else if(maxAirJumps > 0 && _jumpPhase < maxAirJumps) {
            if(_jumpPhase == 0)
                _jumpPhase = 1;
            else
                _jumpPhase++;
            jumpDirection = _groundNormal;
        }
        else {
            input.jump = false;
            return;
        }
        _wallStickTimer = wallStickDelay;
        _coyoteTimer = coyoteTime + 1f;
        _stepsSinceJump = 0;
        float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
        if(LeftRight || _wallStatus == WallStatus.front) {
            jumpDirection = (jumpDirection + tiltRotater.up).normalized;
        }
        Vector3 momentumBoost = _velocity;
        momentumBoost.y = 0f;
        _velocity = momentumBoost;
        _velocity += (jumpDirection * jumpSpeed) + (momentumBoost * jumpMomentum);
        _sliding = false;
        input.slide = false;
    }
    IEnumerator DashDecelerate() {
        _currentSpeed = dashSpeed;
        float speedDiff = (dashSpeed - moveSpeed) / dashDecelerateRate;
        yield return new WaitForSeconds(dashDuration);
        while(_currentSpeed > moveSpeed) {
            _currentSpeed -= speedDiff;
            yield return new WaitForFixedUpdate();
        }
        _currentSpeed = moveSpeed;
    }
    void Dash() {
        Vector3 dashDirection;
        if(DashCooldown && _dashPhase < maxDashes) {
            dashDirection = input.move.normalized;
            Vector3 xDir = dashDirection.x * transform.right;
            Vector3 zDir = dashDirection.y * transform.forward;
            dashDirection = xDir + zDir;
        }
        else {
            input.dash = false;
            return;
        }
        if(!OnGround) {
            _dashPhase++;
            _dashMomentumStacks++;
        }
        else
            _dashMomentumStacks += dashGroundMomentumStacks;
        rb.AddForce(dashDirection * dashSpeed, ForceMode.Impulse);

        dashSound.Play();
        _dashCooldown = dashCooldown;
        _dashDuration = dashDuration;
        input.dash = false;
        StartCoroutine(DashDecelerate());
    }
    void Slide() {
        if(!_sliding && Sliding && _stepsSinceGrounded < 2) {
            slideSound.Play();
            _sliding = true;
            _currentSpeed = slideSpeed;
            slideCollider.enabled = true;
            normalCollider.enabled = false;
            StartCoroutine(SlideDecelerate());
        }
    }
    IEnumerator SlideDecelerate() {
        while(input.slide) {
            if(!OnSlope)
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, slowSlideSpeed, Time.fixedDeltaTime * slideSlowRate);
            else {
                Vector3 zAxis = ProjectDirectionOnPlane(transform.forward, _slopeNormal);
                if(zAxis.z < 0f)
                    _currentSpeed *= slideSlopeMomentum;
                else
                    _currentSpeed /= slideSlopeMomentum;
            }
            yield return new WaitForFixedUpdate();
        }
        float speedDiff = 0f;
        if(_currentSpeed > slideSpeed)
            speedDiff = _currentSpeed - moveSpeed;
        normalCollider.enabled = true;
        slideCollider.enabled = false;
        _currentSpeed = moveSpeed;
        _sliding = false;
        slideSound.Stop();
        _slideMomentumStacks = speedDiff / Time.fixedDeltaTime;
        while(_slideMomentumStacks > 0f) {
            _slideMomentumStacks -= slideMomentumStackDecay;
            yield return new WaitForFixedUpdate();
        }
        _slideMomentumStacks = 0f;
    }
    private void CameraRotation() {
        // if there is an input
        if(input.look.sqrMagnitude >= _threshold) {
            _cinemachineTargetPitch += input.look.y * RotationSpeed * Time.deltaTime;
            _rotationVelocity = input.look.x * RotationSpeed * Time.deltaTime;

            // clamp our pitch rotation
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Update Cinemachine camera target pitch
            CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

            // rotate the player left and right
            transform.Rotate(Vector3.up * _rotationVelocity);

        }

        // Rotates player on x/z axis based on targetRotation, which tilts the camera when wall running or sliding
        Quaternion newTarget = Quaternion.identity;
        newTarget.eulerAngles = new Vector3(targetRotation.eulerAngles.x, tiltRotater.rotation.eulerAngles.y, targetRotation.eulerAngles.z);
        tiltRotater.rotation = Quaternion.Lerp(tiltRotater.rotation, newTarget, Time.deltaTime * tiltSpeed);
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
        if(_wallstickDistance >= _climbDistance)
            _climbable = false;
    }
    void checkWallRun() {
        WallStatus wallStatus;

        //NOTE: Resets wall run states if the player is on the ground
        if(OnGround) {
            _wallStatus = WallStatus.none;
            _firstCling = true;
            _wallstickDistance = 0f;
            _climbable = true;
            TiltPlayer(_wallStatus);
            if(!Sliding)
                _currentSpeed = moveSpeed;
            return;
        }

        //NOTE: Raycasts to check for left/right/front walls
        if(Physics.Raycast(transform.position, transform.right, probeDistance, runMask)) {
            _currentSpeed = runSpeed;
            wallStatus = WallStatus.right;
        }
        else if(Physics.Raycast(transform.position, -transform.right, probeDistance, runMask)) {
            _currentSpeed = runSpeed;
            wallStatus = WallStatus.left;
        }
        else if(Physics.Raycast(transform.position, transform.forward, probeDistance, climbMask) ||
            (wallContact && Physics.Raycast(transform.position, (transform.forward + -transform.up).normalized, 2.5f * probeDistance, climbMask))) {
            _currentSpeed = climbSpeed;
            wallStatus = WallStatus.front;
        }
        else {
            if(!Sliding)
                _currentSpeed = moveSpeed;
            wallStatus = WallStatus.none;
        }
        _wallStatus = wallStatus;
        TiltPlayer(_wallStatus);
    }
    void TiltPlayer(WallStatus status) {
        if(status == WallStatus.front) {
            if(_firstCling) {
                _firstCling = false;
                _wallstickY = transform.position.y;
            }
            wallRunDistanceCheck(transform.position.y);
        }
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
            if(normal.y >= _minGroundDotProduct) {
                _groundContactCount++;
                _groundNormal += normal;
            }
            else if(normal.y >= _minSlopeDotProduct) {
                _slopeContactCount++;
                _slopeNormal += normal;
            }
            else {
                if(normal.y >= _minClimbDotProduct && ((runMask & (1 << layer)) != 0 || (climbMask & (1 << layer)) != 0)) {
                    _climbContactCount++;
                    _climbNormal += normal;
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
