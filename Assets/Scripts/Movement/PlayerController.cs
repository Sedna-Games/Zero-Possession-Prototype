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
    [SerializeField] float moveSpeed = 10.0f;
    [SerializeField] float dashSpeed = 25.0f;
    [SerializeField] float maxSpeed = 40.0f;
    [SerializeField] float maxGroundSpeedChange = 50.0f;
    [SerializeField] float maxAirSpeedChange = 25.0f;
    [SerializeField] float slideSpeed = 17.5f;
    [SerializeField, Tooltip("The speed while sliding lerps from slideSpeed to slowSlideSpeed")]
    float slowSlideSpeed = 17.5f;
    [SerializeField, Tooltip("The amount of seconds to go from fast slide speed to slow slide speed")]
    float slideSlowRate = 5f;
    [SerializeField] float climbSpeed = 10.0f;
    [SerializeField] float wallRunTiltAngle = 15.0f, lateralWallRunTiltAngle = -50.0f, slideTiltAngle = -35.0f, tiltSpeed = 2.5f;
    [SerializeField] int maxAirJumps = 1, maxDashes = 1;
    [SerializeField] float jumpHeight = 2.0f;
    [SerializeField, Tooltip("Extra jump multiplier from walls")]
    float wallJumpMultiplier = 5f;
    [SerializeField, Range(0f, 1f), Tooltip("Percentage speed increase from jumping")]
    float jumpMomentum = 0.05f;
    [SerializeField, Min(1f), Tooltip("Acceleration when sliding down slopes")]
    float slideSlopeMomentum = 1.5f;
    [SerializeField, Range(0f, 1f), Tooltip("Percentage speed increase from dashing")]
    float dashMomentum = 0.05f;
    [SerializeField, Tooltip("Multiplier on dash momentum stacks (base: 1 => airDash = 1, groundDash = 1 * dashGroundMomentumStacks")]
    int dashGroundMomentumStacks = 3;
    [SerializeField, Tooltip("How long you continue to move at dashSpeed (locks normal xz movement)")]
    float dashDuration = 0.5f;
    [SerializeField, Tooltip("Add dashDuration to the value you want to assign this")]
    float dashCooldown = 0.5f;
    [SerializeField, Tooltip("Number of FixedUpdate cycles to lose 1 stack of jump/dash momentum while not in the air (50 cycles/second")]
    int momentumStackDecay = 10;
    [SerializeField, Tooltip("The amount of time after going off an edge that you can still be considered grounded when jumping (doesn't eat air jump)")]
    float coyoteTime = 0.3f;
    float _currentSpeed, _currentClimbSpeed, _totalSpeed;
    float _maxSpeedChange;
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
    [SerializeField] float maxLateralClimbDistance = 16f;
    [SerializeField] LayerMask groundMask = -1, climbMask = -1, slopeMask = -1;
    [SerializeField, Tooltip("How sticky the player is to the wall")]
    float wallForce = 5.0f;
    [SerializeField, Tooltip("How long before the player can stick to the wall after getting off it")]
    float wallStickDelay = 0.1f;
    float _wallStickTimer = 0f;
    float _wallstickDistance;
    float _wallstickY;
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
        _currentClimbSpeed = climbSpeed;
        _maxSpeedChange = maxGroundSpeedChange;
    }
    private void Update() {
        _totalSpeed = (_currentSpeed + (_currentSpeed * jumpMomentum * _jumpMomentumStacks) + (_currentSpeed * dashMomentum * _dashMomentumStacks));
        _desiredVel = new Vector3(input.move.x, 0f, input.move.y) * _totalSpeed;
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
        rb.velocity = _velocity;
        ClearState();
    }
    private void LateUpdate() {
        CameraRotation();
    }
    void ClearState() {
        //??
        //landSound.Play();
        _groundContactCount = _climbContactCount = _slopeContactCount = 0;
        _groundNormal = _climbNormal = _slopeNormal = Vector3.zero;
        _maxSpeedChange = _stepsSinceGrounded < 2 ? maxGroundSpeedChange : maxAirSpeedChange;
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
            if(_stepsSinceJump % momentumStackDecay == 0) {
                _jumpMomentumStacks = Math.Max(0, _jumpMomentumStacks - 1);
            }
            if(_stepsSinceDash % momentumStackDecay == 0) {
                _dashMomentumStacks = Math.Max(0, _dashMomentumStacks - 1);
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
        if(Climbing && input.move.y > 0f && _stepsSinceJump > 1 && _climbable) {
            switch(_wallStatus) {
                case (WallStatus.none):
                    break;
                case (WallStatus.left):
                    _velocity += (wallForce * -Vector3.right);
                    break;
                case (WallStatus.right):
                    _velocity += (wallForce * Vector3.right);
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


        if(Climbing && input.move.y > 0f && _stepsSinceJump > 1) {
            if(_wallStatus == WallStatus.front)
                _velocity = Vector3.up * _currentClimbSpeed;
            else
                _velocity = transform.forward * climbSpeed;

            return;
        }
        if(!wallContact) {
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

            if(InAir) {
                //NOTE: Applies current velocity to appropriate x/z axis
                currentX = Vector3.Dot(_velocity, xAxis);
                currentZ = Vector3.Dot(_velocity, zAxis);
                float acceleration = _currentSpeed * _maxSpeedChange;

                float newX = Mathf.MoveTowards(currentX, _desiredVel.x, acceleration);
                float newZ = Mathf.MoveTowards(currentZ, _desiredVel.z, acceleration);
                _velocity += transform.right * (newX - currentX) + transform.forward * (newZ - currentZ);
            }
            else {
                currentX = Vector3.Dot(_desiredVel, xAxis);
                currentZ = Vector3.Dot(_desiredVel, zAxis);
                _velocity = transform.right * _desiredVel.x + transform.forward * _desiredVel.z + transform.up * _velocity.y;
            }
            Debug.Log(xAxis);
            Debug.Log(zAxis);
            Vector3 capSpeed = new Vector3(_velocity.x, 0f, _velocity.z);
            if(OnSlope && _sliding) {
                capSpeed *= slideSlopeMomentum;
            }
            _velocity = capSpeed.normalized * Mathf.Min(capSpeed.magnitude, maxSpeed) + transform.up * _velocity.y;
        }
    }
    void Jump() {
        Vector3 jumpDirection;
        if(OnGround || _coyoteTimer <= coyoteTime) {
            _jumpPhase = 0;
            jumpDirection = _groundNormal;
            _jumpMomentumStacks++;
        }
        else if(wallContact && _wallStatus != WallStatus.none || _coyoteTimer <= coyoteTime) {
            _jumpPhase = 0;
            jumpDirection = _climbNormal;
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
        jumpDirection = (jumpDirection + tiltRotater.up).normalized;
        float alignedSpeed = Vector3.Dot(_velocity, jumpDirection);
        if(alignedSpeed > 0f & _wallStatus == WallStatus.none) {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }
        Vector3 momentumBoost = rb.velocity;
        momentumBoost.y = 0f;
        _velocity = momentumBoost;
        if(Climbing)
            jumpSpeed *= wallJumpMultiplier;
        _velocity += ((jumpDirection * jumpSpeed) + (momentumBoost * jumpMomentum));
        _sliding = false;
        input.slide = false;
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
            _currentSpeed = Mathf.Lerp(_currentSpeed, slowSlideSpeed, Time.fixedDeltaTime / slideSlowRate);
            yield return new WaitForFixedUpdate();
        }
        normalCollider.enabled = true;
        slideCollider.enabled = false;
        _currentSpeed = moveSpeed;
        _sliding = false;
        slideSound.Stop();
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
        if(_wallstickDistance >= maxLateralClimbDistance)
            _climbable = false;
    }
    void checkWallRun() {
        WallStatus wallStatus;
        if(!_climbable) {
            _wallStatus = WallStatus.none;
            _currentClimbSpeed = Mathf.Lerp(_currentClimbSpeed, -climbSpeed, slideSlowRate * Time.fixedDeltaTime);
        }
        //NOTE: Resets wall run states if the player is on the ground
        if(OnGround) {
            _wallStatus = WallStatus.none;
            _firstCling = true;
            _currentClimbSpeed = climbSpeed;
            _wallstickDistance = 0f;
            _climbable = true;
            TiltPlayer(_wallStatus);
            return;
        }

        //NOTE: Raycasts to check for left/right/front walls
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
            if(_firstCling) {
                _firstCling = false;
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
            if(normal.y >= _minGroundDotProduct) {
                _groundContactCount++;
                _groundNormal += normal;
            }
            else if(normal.y >= _minSlopeDotProduct) {
                _slopeContactCount++;
                _slopeNormal += normal;
            }
            else {
                if(normal.y >= _minClimbDotProduct && (climbMask & (1 << layer)) != 0) {
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
