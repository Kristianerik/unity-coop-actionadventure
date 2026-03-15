using System.Collections;
using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 9f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -19.62f;
    [SerializeField] private LayerMask groundMask;

    [Header("Combat")]
    [SerializeField] private WeaponHandler weaponHandler;

    [Header("Double Tap Dodge")]
    [SerializeField] private float doubleTapWindow = 0.3f;
    [SerializeField] private float dodgeRollSpeed = 15f;
    [SerializeField] private float dodgeRollDuration = 0.3f;
    [SerializeField] private float dodgeRollCooldown = 1f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Camera")]
    [SerializeField] private CameraController cameraController;

    // Components
    private CharacterController _controller;
    private Animator _animator;
    private Transform _cameraTransform;

    // Movement
    private Vector2 _moveInput;
    private Vector3 _velocity;
    private bool _isGrounded;
    private bool _isSprinting;
    private Vector3 _moveDirection;

    // Dodge Roll
    private bool _isDodgeRolling;
    private float _dodgeRollTimer;
    private float _dodgeRollCooldownTimer;
    private Vector3 _dodgeRollDirection;

    // Combat
    private bool _isBlocking;

    // Double Tap
    private float _lastJumpTapTime = -1f;
    private bool _waitingForSecondTap = false;

    // Look
    private Vector2 _lookInput;


    // Animator Hashes
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int DodgeRollHash = Animator.StringToHash("DodgeRoll");
    private static readonly int JumpHash = Animator.StringToHash("Jump");

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _cameraTransform = Camera.main.transform;
        cameraController = Camera.main.GetComponent<CameraController>();
    }

    private void Update()
    {
        HandleGroundCheck();
        HandleDodgeRollTimer();
        HandleMovement();
        HandleRotation();
        UpdateAnimator();
    }

    // Input Callbacks

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        _lookInput = context.ReadValue<Vector2>();
        cameraController?.OnLook(context);

        _lookInput = context.ReadValue<Vector2>();
        cameraController?.OnLook(context);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        float timeSinceLastTap = Time.time - _lastJumpTapTime;

        if (_waitingForSecondTap && timeSinceLastTap <= doubleTapWindow)
        {
            // Double tap detected - cancel jump and dodge roll
            _waitingForSecondTap = false;
            _lastJumpTapTime = -1f;
            StopCoroutine(nameof(ResetDoubleTapWindow));

            if (!_isDodgeRolling && _dodgeRollCooldownTimer <= 0f)
            {
                _velocity.y = -2f; // cancel jump velocity
                StartDodgeRoll();
            }
            return;
        }

        // First tap - jump immediately
        _waitingForSecondTap = true;
        _lastJumpTapTime = Time.time;
        StopCoroutine(nameof(ResetDoubleTapWindow));
        StartCoroutine(nameof(ResetDoubleTapWindow));

        if (_isGrounded && !_isDodgeRolling)
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            if (_animator != null) _animator.SetTrigger(JumpHash);
        }
    }

    private IEnumerator ResetDoubleTapWindow()
    {
        yield return new WaitForSeconds(doubleTapWindow);
        _waitingForSecondTap = false;
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        _isSprinting = context.ReadValueAsButton();
    }

    public void OnLightAttack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        weaponHandler?.LightAttack(_moveInput);
    }

    public void OnHeavyAttack(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            weaponHandler?.HeavyAttack(_moveInput);
        }
        

        if (context.canceled) 
        {
            weaponHandler?.CancelCharge();
        }
    }

    public void OnBlock(InputAction.CallbackContext context)
    {
        _isBlocking = context.ReadValueAsButton();
        weaponHandler?.Block(_isBlocking);
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        // TODO: Trigger interaction - will be implemented in Interaction System
        Debug.Log("Interact");
    }


    // Movement
    
    private void HandleMovement()
    {
        if (_isGrounded && _velocity.y < 0f)
            _velocity.y = -2f;
        else
            _velocity.y += gravity * Time.deltaTime;

        Vector3 move;
        if (_isDodgeRolling)
        {
            move = _dodgeRollDirection * dodgeRollSpeed; // ← must be here
        }
        else
        {
            float speed = _isSprinting ? sprintSpeed : walkSpeed;
            move = GetCameraRelativeMovement() * speed;
        }

        Vector3 finalMove = new Vector3(move.x, _velocity.y, move.z);
        _controller.Move(finalMove * Time.deltaTime);
    }

    private Vector3 GetCameraRelativeMovement()
    {
        Vector3 forward = _cameraTransform.forward;
        Vector3 right = _cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        return forward * _moveInput.y + right * _moveInput.x;
    }

    private void HandleGroundCheck()
    {
        Vector3 spherePosition = new Vector3(
            transform.position.x,
            transform.position.y + 0.1f,
            transform.position.z
        );

        _isGrounded = Physics.CheckSphere(
            spherePosition,
            0.4f,
            groundMask
        );

    }

    private void HandleRotation()
    {
        if (_isDodgeRolling) return;

        // Always match player yaw to camera yaw
        float cameraYaw = _cameraTransform.eulerAngles.y;
        Quaternion targetRotation = Quaternion.Euler(0f, cameraYaw, 0f);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    // Dodge Roll

    private void StartDodgeRoll()
    {
        _isDodgeRolling = true;
        _dodgeRollTimer = dodgeRollDuration;
        _dodgeRollCooldownTimer = dodgeRollCooldown;

        Vector3 move = GetCameraRelativeMovement();
        _dodgeRollDirection = move.sqrMagnitude > 0.01f ? move.normalized : transform.forward;

        if (_animator != null) _animator.SetTrigger(DodgeRollHash);
    }

    private void HandleDodgeRollTimer()
    {
        if (_isDodgeRolling)
        {
            _dodgeRollTimer -= Time.deltaTime;
            if (_dodgeRollTimer <= 0f)
            {
                _isDodgeRolling = false;
            }
        }

        if (_dodgeRollCooldownTimer > 0f)
        {
            _dodgeRollCooldownTimer -= Time.deltaTime;
        }
    }

    // Animator

    private void UpdateAnimator()
    {
        if (_animator == null) return;
        if (_animator.runtimeAnimatorController == null) return;

        float speed = new Vector3(
            _controller.velocity.x, 0f, _controller.velocity.z
        ).magnitude; 

        _animator.SetFloat(SpeedHash, speed, 0.1f, Time.deltaTime);
        _animator.SetBool(IsGroundedHash, _isGrounded);
    }
}
