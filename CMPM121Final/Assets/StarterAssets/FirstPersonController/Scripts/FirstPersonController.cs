using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif
using StarterAssets;

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */
[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
[RequireComponent(typeof(PlayerInput))]
#endif
public class FirstPersonController : MonoBehaviour
{
    [Header("Player")]
    [Tooltip("Move speed of the character in m/s")]
    public float MoveSpeed = 4.0f;
    [Tooltip("Sprint speed of the character in m/s")]
    public float SprintSpeed = 6.0f;
    [Tooltip("Rotation speed of the character")]
    public float RotationSpeed = 1.0f;
    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    [Space(10)]
    [Tooltip("The height the player can jump")]
    public float JumpHeight = 1.2f;
    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float Gravity = -15.0f;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.1f;
    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;
    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;
    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.5f;
    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;
    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 90.0f;
    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -90.0f;

    // grapple
    public bool GrappleReady = true;
    public bool GrappleOut = false;
    public bool GrappleAttached = false;

    // cinemachine
    private float _cinemachineTargetPitch;

    // player
    private float _speed;
    private float _rotationVelocity;
    [HideInInspector] public float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    // timeout deltatime
    private float _jumpTimeoutDelta;
    [HideInInspector] public float _fallTimeoutDelta;

    [HideInInspector] public CharacterController _controller;
    [HideInInspector] public StarterAssetsInputs _input;
    private GameObject _mainCamera;

    private const float _threshold = 0.01f;

    private bool isGrounded; // is on a slope or not
    public float slideFriction = 0.3f; // ajusting the friction of the slope
    private Vector3 hitNormal; //orientation of the slope.
    private float slideGravity;

    [HideInInspector] public bool isDead;
    [HideInInspector] public float launchTime;

    private void Awake()
    {
        // get a reference to our main camera
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

        }
    }

    float mass = 3.0f; // defines the character mass
    Vector3 impact = Vector3.zero;


    // call this function to add an impact force:
    public void AddImpact(Vector3 direction, float force)
    {
        direction.Normalize();
        if (direction.y < 0) direction.y = -direction.y; // reflect down force on the ground
        impact += direction.normalized * force / mass;
    }


    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<StarterAssetsInputs>();

        // reset our timeouts on start
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;

        // grapple input actions
        _input.OnGrappleButton += ShootGrapple;
        _input.OnGrappleRelease += ReleaseGrapple;
    }

    private void Update()
    {
        JumpAndGravity();
        if(launchTime <= 0)
        {
            GroundedCheck();
        }
        else
        {
            launchTime -= Time.deltaTime;
        }
        Move();

    }

    private void FixedUpdate()
    {
        //JumpAndGravity();
        // GroundedCheck();
        //Move();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        hitNormal = hit.normal;
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
    }

    private void CameraRotation()
    {
        // if there is an input
        if (_input.look.sqrMagnitude >= _threshold)
        {
            _cinemachineTargetPitch += _input.look.y * RotationSpeed * Time.deltaTime;
            _rotationVelocity = _input.look.x * RotationSpeed * Time.deltaTime;

            // clamp our pitch rotation
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Update Cinemachine camera target pitch
            CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

            // rotate the player left and right
            transform.Rotate(Vector3.up * _rotationVelocity);
        }
    }

    private void Move()
    {
        // set target speed based on move speed, sprint speed and if sprint is pressed
        float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

        // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

        // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is no input, set the target speed to 0
        if (_input.move == Vector2.zero) targetSpeed = 0.0f;

        // a reference to the players current horizontal velocity
        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

        if (inputMagnitude > 0)
        {
            _speed = Mathf.Lerp(_speed, targetSpeed, SpeedChangeRate * Time.deltaTime);
        }
        else
        {
            _speed = Mathf.Lerp(_speed, 0, SpeedChangeRate * Time.deltaTime);
        }

        // normalise input direction
        Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

        // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is a move input rotate player when the player is moving
        if (_input.move != Vector2.zero)
        {
            // move
            inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
        }

        // move the player
        _controller.Move(inputDirection.normalized * (_speed * (Grounded || impact.magnitude == 0 ? 1 : 2) * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime + impact * Time.deltaTime);

        // consumes the impact energy each cycle:
        impact = Vector3.Lerp(impact, Vector3.zero, Time.deltaTime);

        isGrounded = Vector3.Angle(Vector3.up, hitNormal) <= _controller.slopeLimit;
        if (!isGrounded && Grounded)
        {
            Vector3 m_MoveDir = new Vector3(0, 0, 0);
            slideGravity = Mathf.Clamp(slideGravity += 9 * Time.deltaTime, 3, 30);
            m_MoveDir.x += (1f - hitNormal.y) * hitNormal.x * (slideGravity - slideFriction);
            m_MoveDir.z += (1f - hitNormal.y) * hitNormal.z * (slideGravity - slideFriction);
            _controller.Move(m_MoveDir * Time.deltaTime);
        }
        else
        {
            slideGravity = 0;
        }
    }

    private void JumpAndGravity()
    {
        if (Grounded)
        {
            // reset the fall timeout timer
            _fallTimeoutDelta = FallTimeout;

            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // Jump
            if (_input.jump && _jumpTimeoutDelta <= 0.0f)
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
            }

            // jump timeout
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }

            impact = Vector3.zero;

        }
        else
        {
            // reset the jump timeout timer
            _jumpTimeoutDelta = JumpTimeout;

            // fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }

            // if we are not grounded, do not jump
            _input.jump = false;
        }

        // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    private void ShootGrapple()
    {
        if (GrappleReady)
        {
            //Debug.Log("Shoot Grapple");
            GrappleReady = false;
            GrappleOut = true;
        }
    }

    private void ReleaseGrapple()
    {
        //Debug.Log("Release Grapple");
        GrappleOut = false;
        GrappleAttached = false;
        GrappleReady = true;
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (Grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
    }
}
