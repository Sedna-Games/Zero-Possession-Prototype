using System;
using System.Collections;
using System.Collections.Generic;
using InputManagerScript;
using Unity.Mathematics;
using Unity.VisualScripting;
//using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour {
    [Header("Sounds")]
    [SerializeField] FMODUnity.StudioEventEmitter footSteps = null;
    [SerializeField] FMODUnity.StudioEventEmitter dashSound = null;
    [SerializeField] FMODUnity.StudioEventEmitter dashResetSound = null;
    [SerializeField] FMODUnity.StudioEventEmitter JumpSound = null;
    [SerializeField] FMODUnity.StudioEventEmitter doubleJumpSound = null;
    [SerializeField] FMODUnity.StudioEventEmitter moveFastSound = null;
    [SerializeField] FMODUnity.StudioEventEmitter slideSound = null;
    [SerializeField] FMODUnity.StudioEventEmitter landSound = null;
    [SerializeField, Range(0f, 1f), Tooltip("Percentage of max speed that the player needs to be going at to play moveFastSound (loop)")]
    float moveFastRate = 0.8f;
    [SerializeField, Min(0.01f), Tooltip("Seconds between footstep noises, multiplied by inverse of percentage of max speed (faster = faster footsteps)")]
    float footStepRate = 2.5f;
    [SerializeField, Range(0.01f, 1f), Tooltip("Determines how strong the bias of the player's speed compared to the max speed is")]
    float speedEffectOnFootsteps = 0.5f;
    [SerializeField, Tooltip("Amount of time spent in the air before landing can play its sound")]
    float airTimeForLanding = 0.2f;
    float percentOfMaxSpeed => _totalSpeed / maxSpeed;
    int _stepsToFootsteps = 0;

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
    float ManeuverabilityRate = 1f, AirManeuverabilityRate = 1.0f, GroundManeuverabilityRate = 1.0f;
    [SerializeField] float wallRunTiltAngle = 15.0f, lateralWallRunTiltAngle = -50.0f, slideTiltAngle = -35.0f, tiltSpeed = 2.5f;
    [SerializeField] int maxAirJumps = 1, maxDashes = 1;
    [SerializeField] float jumpHeight = 2.0f;

    [SerializeField, Tooltip("Delay before you can move immediately after jumping off a wall")]
    float JumpMovementDelay = 0.5f;
    [SerializeField, Tooltip("Extra jump multiplier from walls")]
    float wallJumpMultiplier = 5f;
    [SerializeField, Range(0f, 1f), Tooltip("Percentage speed increase from jumping")]
    float jumpMomentum = 0.05f;
    [SerializeField, Min(1f), Tooltip("Acceleration when sliding down slopes")]
    float slideSlopeMomentum = 1.1f;
    [SerializeField, Tooltip("Extra speed converted from sliding -> running that decays (Slide stacks = speedDiff / slideCarryOverMomentum)"), Range(0.01f, 1f)]
    float slideCarryOverMomentum = 0.02f;
    [SerializeField, Tooltip("How many slide stacks you lose per fixed updated cycle (Combine with above to control how quickly you lose it)")]
    float slideMomentumStackDecay = 3f;
    [SerializeField, Range(0f, 1f), Tooltip("Percentage speed increase from dashing")]
    float dashMomentum = 0.05f;
    [SerializeField, Tooltip("Multiplier on dash momentum stacks (base: 1 => airDash = 1, groundDash = 1 * dashGroundMomentumStacks")]
    float dashGroundMomentumStacks = 3;
    [SerializeField, Tooltip("How long you continue to move at dashSpeed (locks normal xz movement)")]
    float dashDuration = 0.5f;
    [SerializeField, Tooltip("Add dashDuration to the value you want to assign this")]
    float dashCooldown = 0.5f;
    [SerializeField, Tooltip("Amount of fixedUpdate cycles to go from dashSpeed to moveSpeed")]
    float dashDecelerateRate = 50f;
    [SerializeField, Tooltip("Number of FixedUpdate cycles to lose jump/dashMomentumDecay stacks of jump/dash momentum while not in the air (50 cycles/second")]
    int momentumStackDecay = 10;
    [SerializeField, Tooltip("Amount of stacks to remove once the above happens")]
    float jumpMomentumDecay = 1f, dashMomentumDecay = 1f;
    [SerializeField, Tooltip("The difficulty of stacking momentum (each action gives you 1f / (actionMomentumStacks * momentumStackingDifficulty) stacks). To disable, set to 0f"), Range(0f, 1f)]
    float momentumStackingDifficulty = 1f;
    [SerializeField, Tooltip("The amount of time after going off an edge that you can still be considered grounded when jumping (doesn't eat air jump)")]
    float coyoteTime = 0.3f;

    [SerializeField, Tooltip("Invokes a UnityEvent that resets the player")]
    UnityEvent reloadEvent;
    [Header("Lunge"), SerializeField, Tooltip("Lunge duration, adjust with lungeForce")]
    float animationLock = 0.2f;
    [SerializeField, Tooltip("Distance to the enemy to lunge towards")]
    float lungeDist = 10.0f;
    [SerializeField, Tooltip("Size of the lunge spherecast")]
    float lungeRadius = 1.0f;
    [SerializeField, Tooltip("Lunge force towards the enemy")]
    float lungeForce = 50f;
    [SerializeField, Tooltip("Layers to check for lunge targets")]
    LayerMask lungeMask = 7;
    [SerializeField, Tooltip("Invokes a UnityEvent if the lunge is successful (i.e. flag to stop taking damage from Health script)")]
    UnityEvent LungeEvent;
    bool lungeCameraLock = false;
    float _currentSpeed, _currentClimbSpeed;
    float _totalSpeed => calculateMomentumStacks(_currentSpeed);

    float _climbDistance => calculateMomentumStacks(maxClimbDistance);
    float _slideMomentumStacks;
    float _dashCooldown = 0.0f;
    float _dashDuration = 0f;
    float _coyoteTimer = 0f;
    bool _desireJump = false, _desireDash = false, _movementDelay = false, reverseSlide = false;
    Quaternion targetRotation;

    [Header("Terrain Settings"), Space(10)]

    [SerializeField, Tooltip("Distance for ground/wall raycasts"), Min(0f)]
    float probeDistance = 1f;
    [SerializeField, Tooltip("Maximum speed that snapping will work")]
    float maxSnapSpeed = 25f;
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
    float _wallstickDistance, _wallstickY = float.MaxValue - 0.1f;
    bool _firstCling = true, _climbable = false;

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    [SerializeField] GameObject CinemachineCameraTarget;
    [Tooltip("How far in degrees can you move the camera up")]
    [SerializeField] float TopClamp = 90.0f;
    [Tooltip("How far in degrees can you move the camera down")]
    [SerializeField] float BottomClamp = -90.0f;
    [Tooltip("Rotation speed of the character")]
    [SerializeField] public float RotationSpeed = 1.0f;
    [SerializeField, Tooltip("Number of seconds to wait at the start before updating the camera to prevent looking in a random direction at start")]
    float stopDuration = 5f;
    bool stopUpdateCamera = false;
    private float _rotationVelocity;
    private float _cinemachineTargetPitch;
    private const float _threshold = 0.01f;
    bool _inCutscene = false;

    [Header("Assets"), Space(10)]
    [SerializeField] Rigidbody rb;
    [SerializeField] InputManager input;
    [SerializeField] Transform tiltRotater;
    [SerializeField] Collider normalCollider;
    [SerializeField] Collider slideCollider;
    Vector3 _velocity, _desiredVel;
    int _jumpPhase, _dashPhase;
    float _jumpMomentumStacks = 0, _dashMomentumStacks = 0;
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
    public bool StandingStill => rb.velocity.magnitude <= Mathf.Epsilon;
    public bool isRunning = false;

    WallStatus _wallStatus;
    contactState _lastContact;
    bool speedingCoroutine = false;

    //UI Stuff
    public float dashFill => (1f - Mathf.Max(0f, _dashCooldown) / dashCooldown);
    public bool inLungeRange => Physics.CapsuleCast(transform.position, transform.position, lungeRadius, CinemachineCameraTarget.transform.forward, lungeDist, lungeMask);

    private void Awake() {
        OnValidate();
    }

    private void OnEnable() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        IEnumerator stopCameraFromUpdatingAtStart() {
            stopUpdateCamera = true;
            yield return new WaitForSecondsRealtime(stopDuration);
            stopUpdateCamera = false;
        }
        StartCoroutine(stopCameraFromUpdatingAtStart());
    }

    void OnValidate() {
        _minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        _minClimbDotProduct = Mathf.Cos(maxClimbAngle * Mathf.Deg2Rad);
        _minSlopeDotProduct = Mathf.Cos(maxSlopeAngle * Mathf.Deg2Rad);
        _currentSpeed = moveSpeed;
        _currentClimbSpeed = climbSpeed;
    }
    IEnumerator PlaySpeedSounds() {
        moveFastSound.Play();
        while(percentOfMaxSpeed > moveFastRate) {
            yield return new WaitForFixedUpdate();
        }
        moveFastSound.Stop();
        speedingCoroutine = false;
    }
    //NOTE: Calculates actual speed after applying all momentum stacks. Sliding uses a different setup and is based on the const Time.fixedDeltaTime instead
    float calculateMomentumStacks(float speed) {
        float newSpeed = speed + (speed * jumpMomentum * _jumpMomentumStacks) + (speed * dashMomentum * _dashMomentumStacks) + (_slideMomentumStacks * slideCarryOverMomentum);
        if(newSpeed / maxSpeed > moveFastRate && !speedingCoroutine) {
            speedingCoroutine = true;
            StartCoroutine(PlaySpeedSounds());
        }
        return newSpeed;
    }
    public void resetMomentumStacks(bool deathReset = false) {
        _jumpMomentumStacks = 0f;
        _dashMomentumStacks = 0f;
        _slideMomentumStacks = 0f;
        if(deathReset) {
            _dashDuration = 0f;
            _dashCooldown = dashCooldown;
            _jumpPhase = 0;
        }
    }
    public void lockMovement(bool yn) {
        _inCutscene = yn;
    }

    bool SnapToGround() {
        RaycastHit hit;
        if(_stepsSinceGrounded > 1 || _stepsSinceJump <= 2)
            return false;
        float speed = _velocity.magnitude;
        if(speed > maxSnapSpeed)
            return false;
        LayerMask masks = groundMask | climbMask | slopeMask;
        if(!Physics.Raycast(transform.position, Vector3.down, out hit, probeDistance, masks))
            return false;
        float contactDotProduct = 0f;
        switch(_lastContact) {
            case contactState.ground:
                contactDotProduct = _minGroundDotProduct;
                break;
            case contactState.slope:
                contactDotProduct = _minSlopeDotProduct;
                break;
            case contactState.wall:
                contactDotProduct = _minClimbDotProduct;
                break;
        }
        if(hit.normal.y < contactDotProduct)
            return false;

        switch(_lastContact) {
            case contactState.ground:
                _groundContactCount = 1;
                _groundNormal = hit.normal;
                break;
            case contactState.slope:
                _slopeContactCount = 1;
                _slopeNormal = hit.normal;
                break;
            case contactState.wall:
                _climbContactCount = 1;
                _climbNormal = hit.normal;
                break;
        }
        float dot = Vector3.Dot(_velocity, hit.normal);
        if(dot > 0f)
            _velocity = (_velocity - hit.normal * dot).normalized * speed;
        return true;
    }
    void addJumpMomentumStacks() {
        if(momentumStackingDifficulty == 0f || _jumpMomentumStacks == 0f)
            _jumpMomentumStacks += 1f;
        else
            _jumpMomentumStacks += 1f / Mathf.Ceil(_jumpMomentumStacks * momentumStackingDifficulty);
    }
    void addDashMomentumStacks(bool groundDash = false) {
        if(momentumStackingDifficulty == 0f || _dashMomentumStacks == 0f)
            _dashMomentumStacks += 1f;
        else if(groundDash)
            _dashMomentumStacks += dashGroundMomentumStacks / Mathf.Ceil(_dashMomentumStacks * momentumStackingDifficulty);
        else
            _dashMomentumStacks += 1.0f / Mathf.Ceil(_dashMomentumStacks * momentumStackingDifficulty);
    }
    private void Update() {
        if(_dashCooldown - Time.deltaTime < 0f && !DashCooldown)
            dashResetSound.Play();
        _dashCooldown -= Time.deltaTime;
        _dashDuration -= Time.deltaTime;
        _wallStickTimer -= Time.deltaTime;
        //NOTE: The input manager updates in Update() instead of FixedUpdate() so this helps keep consistency for button presses
        if(!_inCutscene) {
            _desiredVel = new Vector3(input.move.x, 0f, input.move.y).normalized * _totalSpeed;
            if(input.jump) {
                _desireJump = true;
                input.jump = false;
            }
            if(input.dash) {
                _desireDash = true;
                input.dash = false;
            }
            if(input.reload) {
                input.reload = false;
                reloadEvent.Invoke();
            }
        }
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
        CameraRotation();
    }

    //NOTE: More control over gravity and removes sliding downwards on slopes
    void ApplyGravity() {
        if(OnSlope)
            _velocity += gravity * _slopeNormal * Time.fixedDeltaTime;
        else if(!OnGround)
            _velocity += gravity * _groundNormal * Time.fixedDeltaTime;
    }
    void ClearState() {
        if(_climbContactCount > 0)
            _lastContact = contactState.wall;
        else if(_slopeContactCount > 0)
            _lastContact = contactState.slope;
        else if(_groundContactCount > 0)
            _lastContact = contactState.ground;
        _groundContactCount = _climbContactCount = _slopeContactCount = 0;
        _groundNormal = _climbNormal = _slopeNormal = Vector3.zero;
    }
    void UpdateState() {
        var landing = _stepsSinceGrounded;
        _velocity = rb.velocity;
        _stepsSinceJump++;
        _stepsSinceDash++;
        _stepsSinceGrounded++;

        isRunning = (OnGround || landing < (airTimeForLanding * 0.4f) / Time.fixedDeltaTime) && !Sliding && !Dashing && !StandingStill;

        if(OnGround || SnapToGround() || (wallContact && _wallStatus != WallStatus.none) || OnSlope) {
            _stepsToFootsteps++;
            if(_stepsToFootsteps % (int)(Mathf.Max(0.1f, 1f - percentOfMaxSpeed) / speedEffectOnFootsteps * footStepRate / Time.fixedDeltaTime) == 0 && _velocity.magnitude >= 0.1f && !Sliding)
                footSteps.Play();
            _dashPhase = 0;
            _coyoteTimer = 0f;
            _stepsSinceGrounded = 0;
            if(_stepsSinceJump > 1) {
                _jumpPhase = 0;
                _coyoteTimer = 0f;
            }
            if(landing > _stepsSinceGrounded && landing > airTimeForLanding / Time.fixedDeltaTime)
                landSound.Play();
            //NOTE: Momentum decay. Doesn't decay for wall running but decays for ground, slope, and climbing
            if(_wallStatus != WallStatus.left && _wallStatus != WallStatus.right) {
                if(_stepsSinceJump % momentumStackDecay == 0) {
                    _jumpMomentumStacks = Mathf.Max(0f, _jumpMomentumStacks - jumpMomentumDecay);
                }
                if(_stepsSinceDash % momentumStackDecay == 0) {
                    _dashMomentumStacks = Mathf.Max(0f, _dashMomentumStacks - dashMomentumDecay);
                }
            }
            if(_groundContactCount > 1)
                _groundNormal.Normalize();
            else //NOTE: Need this for gravity to prevent clinging on walls
                _groundNormal = Vector3.up;
            if(_climbContactCount > 1)
                _climbNormal.Normalize();
            if(_slopeContactCount > 1)
                _slopeNormal.Normalize();
        }
        else {
            _coyoteTimer += Time.fixedDeltaTime;
            _stepsToFootsteps = 0;
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
        else if(_movementDelay)
            return;
        //NOTE: Based on contact normal, returns the parallel direction for x/z axis
        if(OnSlope) {
            xAxis = ProjectDirectionOnPlane(transform.right, _slopeNormal);
            zAxis = ProjectDirectionOnPlane(transform.forward, _slopeNormal);
        }
        else {
            xAxis = ProjectDirectionOnPlane(transform.right, _groundNormal);
            zAxis = ProjectDirectionOnPlane(transform.forward, _groundNormal);
        }

        //NOTE: Resets momentum stacks if player isn't moving
        if(_velocity.x + _velocity.z < 0.5f && _desiredVel.magnitude == 0f)
            resetMomentumStacks();

        //NOTE: Makes movement less instantaneous while mid-air
        currentX = Vector3.Dot(_velocity, xAxis);
        currentZ = Vector3.Dot(_velocity, zAxis);
        float acceleration = _totalSpeed * GroundManeuverabilityRate * (ManeuverabilityRate / Time.fixedDeltaTime);

        if(InAir)
            acceleration = _totalSpeed * AirManeuverabilityRate * (ManeuverabilityRate / Time.fixedDeltaTime);
        if(reverseSlide)
            _desiredVel = new Vector3(_desiredVel.x, 0f, _desiredVel.z * -1f);

        float newX = Mathf.MoveTowards(currentX, _desiredVel.x, acceleration * Time.fixedDeltaTime);
        float newZ = Mathf.MoveTowards(currentZ, _desiredVel.z, acceleration * Time.fixedDeltaTime);
        _velocity += transform.right * (newX - currentX) + transform.forward * (newZ - currentZ);
        if(Climbing && LeftRight && input.move.y > 0f) {
            float rotateDir = _wallStatus == WallStatus.right ? 90f : -90f;
            Vector3 wallrunDir = Quaternion.Euler(0f, rotateDir, 0f) * _climbNormal;
            _velocity = wallrunDir * _totalSpeed + (wallForce * -_climbNormal);
            //_velocity = new Vector3(_velocity.x, 0f, _velocity.z);
        }
        else if(wallContact && (_wallStatus == WallStatus.front) && input.move.y > 0f && _climbable)
            _velocity = new Vector3(_velocity.x, _desiredVel.z, 0f);

        Vector3 capSpeed = new Vector3(_velocity.x, 0f, _velocity.z);
        _velocity = capSpeed.normalized * Mathf.Min(capSpeed.magnitude, maxSpeed) + transform.up * Mathf.Min(_velocity.y, maxSpeed);
        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("player_speed", percentOfMaxSpeed);
    }
    void Jump() {
        Vector3 jumpDirection;
        if(OnGround) {
            _jumpPhase = 0;
            jumpDirection = _groundNormal;
            addJumpMomentumStacks();
            JumpSound.Play();
        }
        else if(OnSlope) {
            _jumpPhase = 0;
            jumpDirection = _slopeNormal;
            addJumpMomentumStacks();
            JumpSound.Play();
        }
        else if(Climbing) {
            _jumpPhase = 0;
            jumpDirection = _climbNormal;
            addJumpMomentumStacks();
            JumpSound.Play();
        }
        else if(_coyoteTimer <= coyoteTime) {
            _jumpPhase = 0;
            if(_lastContact == contactState.wall) {
                jumpDirection = _climbNormal;
            }
            else
                jumpDirection = _groundNormal;
            addJumpMomentumStacks();
            JumpSound.Play();
        }
        else if(maxAirJumps > 0 && _jumpPhase < maxAirJumps) {
            if(_jumpPhase == 0)
                _jumpPhase = 1;
            else
                _jumpPhase++;
            jumpDirection = _groundNormal;
            doubleJumpSound.Play();
        }
        else {
            input.jump = false;
            return;
        }
        _wallStickTimer = wallStickDelay;
        _stepsSinceJump = 0;
        float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
        if(LeftRight || (_lastContact == contactState.wall && _coyoteTimer <= coyoteTime)) {
            jumpDirection = (jumpDirection + tiltRotater.up).normalized;
            jumpDirection = new Vector3(jumpDirection.x * wallJumpMultiplier, jumpDirection.y * wallJumpMultiplier / 2f, jumpDirection.z * wallJumpMultiplier);
            StartCoroutine(DisableMovement());
        }
        else if(_wallStatus == WallStatus.front) {
            jumpDirection = (jumpDirection + tiltRotater.up).normalized;
            jumpDirection = new Vector3(jumpDirection.x * wallJumpMultiplier / 2f, jumpDirection.y * wallJumpMultiplier / 2f, jumpDirection.z * wallJumpMultiplier / 2f);
            StartCoroutine(DisableMovement());
        }
        _coyoteTimer = coyoteTime + 1f;
        Vector3 momentumBoost = _velocity;
        momentumBoost.y = 0f;
        _velocity = momentumBoost;
        //NOTE: Disables momentum boost when jumping off walls so the player doesn't go too far from the wall when jumping
        _velocity += (jumpDirection * jumpSpeed) + (!(LeftRight || _wallStatus == WallStatus.front)).GetHashCode() * (momentumBoost * jumpMomentum);
        _sliding = false;
        input.slide = false;
    }
    IEnumerator DisableMovement() {
        _movementDelay = true;
        yield return new WaitForSeconds(JumpMovementDelay);
        _movementDelay = false;
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
            addDashMomentumStacks();
        }
        else
            addDashMomentumStacks(true);
        rb.AddForce(dashDirection * dashSpeed, ForceMode.Impulse);

        dashSound.Play();
        _dashCooldown = dashCooldown;
        _dashDuration = dashDuration;
        input.dash = false;
        StartCoroutine(DashDecelerate());
    }
    public void Lunge() {
        if(inLungeRange) {
            lungeCameraLock = true;
            rb.AddForce(CinemachineCameraTarget.transform.forward * lungeForce, ForceMode.Impulse);
            _dashDuration = animationLock;
            LungeEvent.Invoke();

            IEnumerator WaitForLunge() {
                yield return new WaitForSeconds(animationLock);
                lungeCameraLock = false;
            }
            StartCoroutine(WaitForLunge());
        }
    }
    void Slide() {
        if(!_sliding && Sliding && _stepsSinceGrounded < 2) {
            slideSound.Play();
            _sliding = true;
            reverseSlide = false;
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
                float delta = _currentSpeed * (slideSlopeMomentum - 1f);
                Vector3 zAxis = ProjectDirectionOnPlane(transform.forward, _slopeNormal);
                if(zAxis.y > 0f) {
                    if(_currentSpeed < 0.5f)
                        reverseSlide = true;

                    if(reverseSlide)
                        _currentSpeed += delta;
                    else
                        _currentSpeed -= delta;
                }
                else {
                    _currentSpeed += delta;
                    reverseSlide = false;
                }
            }
            yield return new WaitForFixedUpdate();
        }
        reverseSlide = false;
        float speedDiff = 0f;
        if(_currentSpeed > slideSpeed)
            speedDiff = _currentSpeed - moveSpeed;
        normalCollider.enabled = true;
        slideCollider.enabled = false;
        _currentSpeed = moveSpeed;
        _sliding = false;
        slideSound.Stop();
        _slideMomentumStacks = speedDiff / slideCarryOverMomentum;
        while(_slideMomentumStacks > 0f) {
            _slideMomentumStacks = Mathf.Max(0f, _slideMomentumStacks - slideMomentumStackDecay);
            yield return new WaitForFixedUpdate();
        }
        _slideMomentumStacks = 0f;
    }
    private void CameraRotation() {
        if(stopUpdateCamera)
            return;
        // if there is an input
        if(input.look.sqrMagnitude >= _threshold && !lungeCameraLock && Time.timeScale != 0) {
            _cinemachineTargetPitch += input.look.y * RotationSpeed * 0.01f;
            _rotationVelocity = input.look.x * RotationSpeed * 0.01f;

            // clamp our pitch rotation
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Update Cinemachine camera target pitch
            CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

            // rotate the player left and right
            transform.Rotate(Vector3.up * _rotationVelocity);

        }

        // Rotates a tilt transform on x/z axis based on targetRotation, which tilts the camera when wall running or sliding
        Quaternion newTarget = Quaternion.identity;
        newTarget.eulerAngles = new Vector3(targetRotation.eulerAngles.x, tiltRotater.rotation.eulerAngles.y, targetRotation.eulerAngles.z);
        tiltRotater.rotation = Quaternion.Lerp(tiltRotater.rotation, newTarget, Time.smoothDeltaTime * tiltSpeed);
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
            _currentClimbSpeed = climbSpeed * (1f - _wallstickDistance / _climbDistance);
        }
        if(_wallstickDistance >= _climbDistance)
            _climbable = false;
    }
    void checkWallRun() {
        WallStatus wallStatus;

        //NOTE: Resets wall run states if the player is on the ground
        if(OnGround) {
            _firstCling = true;
            _wallstickDistance = 0f;
            _climbable = true;
            if(!Sliding)
                _currentSpeed = moveSpeed;
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
            _currentSpeed = _currentClimbSpeed;
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
        if(status == WallStatus.front && !OnGround) {
            if(_firstCling) {
                _firstCling = false;
                _wallstickY = transform.position.y;
            }
            wallRunDistanceCheck(transform.position.y);
        }
        switch(status) {
            case (WallStatus.none):
                if(_sliding)
                    targetRotation = Quaternion.Euler(slideTiltAngle, 0f, 0f);
                else
                    targetRotation = Quaternion.identity;
                break;
            case (WallStatus.left):
                targetRotation = Quaternion.Euler(0f, 0f, -wallRunTiltAngle);
                break;
            case (WallStatus.right):
                targetRotation = Quaternion.Euler(0f, 0f, wallRunTiltAngle);
                break;
            case (WallStatus.front):
                targetRotation = Quaternion.Euler(lateralWallRunTiltAngle, 0f, 0f);
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
    enum contactState {
        ground = 0,
        slope = 1,
        wall = 2,
    }
}
