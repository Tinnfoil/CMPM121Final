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
public class FirstPersonControllerGrapple : MonoBehaviour
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

    public float drag;

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
    public float GrappleStrength = 10.0f;
    public bool GrappleReady = true;
    public bool GrappleOut = false;
    public bool GrappleAttached = false;

    public Vector3 GrapplePoint;
    public Vector3 GrappleForce;

    public Vector3 ExternalForce;

    public GameObject GrappleHook;
    public GameObject GrappleCable;

    public GameObject Gun;
    public Transform BulletOrigin;

    public Vector3 LookAtPoint;

    public ParticleSystem MuzzleFlash;
    public ParticleSystem BulletParticle;

    // cinemachine
    private float _cinemachineTargetPitch;

    // player
    private float _speed;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    private CharacterController _controller;
    private StarterAssetsInputs _input;
    private GameObject _mainCamera;

    private const float _threshold = 0.01f;

    private bool isGrounded; // is on a slope or not
    public float slideFriction = 0.3f; // ajusting the friction of the slope
    private Vector3 hitNormal; //orientation of the slope.
    private float slideGravity;

    private void Awake()
    {
        // get a reference to our main camera
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");

        }
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

        // gun input actions
        _input.OnGunButton += ShootGun;

    }

    private void Update()
    {
        JumpAndGravity();
        GroundedCheck();

        drag = Grounded ? 10.0f : 6.0f;

        UpdateLookAtPoint();

        DoGrapple();
        Move();

        if (GrappleAttached)
        {
            Debug.DrawRay(transform.position, GrapplePoint - transform.position, Color.green, 0);
            GrappleCable.SetActive(true);
            GrappleHook.SetActive(false);

            GrappleCable.transform.localScale = new Vector3(1, 1, Vector3.Magnitude(GrapplePoint - GrappleCable.transform.position));
            GrappleCable.transform.LookAt(GrapplePoint, Vector3.up);

        }
        else
        {
            GrappleCable.SetActive(false);
            GrappleHook.SetActive(true);
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        hitNormal = hit.normal;
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void UpdateLookAtPoint()
    {
        RaycastHit hit;
        if (Physics.Raycast(_mainCamera.transform.position, _mainCamera.transform.forward, out hit, Mathf.Infinity))
        {
            LookAtPoint = hit.point;
        }
        else
        {
            LookAtPoint = _mainCamera.transform.position + _mainCamera.transform.forward * 1000;
        }
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
        if (Grounded)
        {
            GrappleForce *= .99f;
        }
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

        if (GrappleAttached) _verticalVelocity = 0.0f;

        // move the player
        if (!GrappleAttached)
        {
            // normal movement
            _controller.Move(inputDirection.normalized * (_speed * (Grounded ? 1 : 2) * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime + GrappleForce * Time.deltaTime);
        }
        else
        {
            slideGravity = 0;
            // movement when grappled
            _controller.Move(GrappleForce * Time.deltaTime);
        }

        // depletes grapple force every frame
        if (!GrappleAttached) GrappleForce = Vector3.MoveTowards(GrappleForce, Vector3.zero, drag * Time.deltaTime);

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
        if (_verticalVelocity < _terminalVelocity && !GrappleAttached)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    private void ShootGun()
    {
        Debug.Log("Shoot Gun");

        if (!Gun.GetComponent<Gun>().GunReady) return;

        MuzzleFlash.Play();

        Gun.GetComponent<Animator>().SetTrigger("ShootGun");

        RaycastHit hit;
        if (Physics.SphereCast(_mainCamera.transform.position, .75f, _mainCamera.transform.forward, out hit, Mathf.Infinity))
        {
            if (hit.collider)
            {
                // gun hit something
                if (hit.collider.GetComponentInParent<Trigger>())
                {
                    hit.collider.GetComponentInParent<Trigger>().TriggerObject();
                }
                Debug.DrawRay(BulletOrigin.position, hit.point - BulletOrigin.position, Color.red, 0.1f);
                BulletParticle.gameObject.transform.LookAt(hit.point, Vector3.up);
            }
            else
            {
                // gun did not hit something
                BulletParticle.gameObject.transform.LookAt(LookAtPoint);
            }
        }

        // BulletParticle.Play();
    }

    private void ShootGrapple()
    {
        // shoot grapple out
        // check if an object was hit
        if (GrappleReady)
        {
            Debug.Log("Shoot Grapple");
            GrappleReady = false;
            GrappleOut = true;

            RaycastHit hit;
            if (Physics.SphereCast(_mainCamera.transform.position, .35f, _mainCamera.transform.forward, out hit, 30.0f))
            {
                GrapplePoint = hit.point;
                GrappleAttached = true;
                Debug.Log(hit.point);
            }
        }
    }

    private void ReleaseGrapple()
    {
        // bring the grapple back to the player
        Debug.Log("Release Grapple");
        GrappleOut = false;
        GrappleAttached = false;
        GrappleReady = true;
    }

    private void DoGrapple()
    {
        if (GrappleAttached)
        {
            Vector3 direction = GrapplePoint - transform.position;

            if (direction.magnitude < 0.5f)
            {
                GrappleForce = Vector3.zero;
                return;
            }

            direction.Normalize();

            GrappleForce += Time.deltaTime * direction.normalized * GrappleStrength / 6.0f;
        }
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    private void OnDrawGizmosSelected()
    {
        /*
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (Grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);

        if (Vector3.Magnitude(LookAtPoint - _mainCamera.transform.position) <= 20) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        Gizmos.DrawSphere(LookAtPoint, GroundedRadius);
        */
    }
}
