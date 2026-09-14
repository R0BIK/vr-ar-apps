using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Рух")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    [Header("Огляд мишкою")]
    public float mouseSensitivity = 1.5f;
    public Transform playerCamera;

    [Header("Перевірка підлоги")]
    public float groundCheckRadius = 0.25f;

    private Rigidbody _rb;
    private CapsuleCollider _col;
    private bool _isGrounded;
    private float _lastJumpTime;
    private float _xRotation;
    private bool _hasCamera;
    private readonly Collider[] _groundHits = new Collider[8];

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _col = GetComponent<CapsuleCollider>();
        _rb.constraints = RigidbodyConstraints.FreezeRotation;

        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }

        _hasCamera = playerCamera != null;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleMouseLook();

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Time.time - _lastJumpTime > 0.15f && _rb.linearVelocity.y <= 0.1f)
        {
            Vector3 spherePos = transform.position + Vector3.down * (_col.height * 0.5f - groundCheckRadius + 0.05f);
            int hitCount = Physics.OverlapSphereNonAlloc(spherePos, groundCheckRadius, _groundHits);

            _isGrounded = false;
            for (int i = 0; i < hitCount; i++)
            {
                var hit = _groundHits[i];
                if (hit != _col && !hit.isTrigger)
                {
                    _isGrounded = true;
                    break;
                }
            }
        }
        else
        {
            _isGrounded = false;
        }

        bool jump = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;

        if (jump && _isGrounded)
        {
            _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
            _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            _isGrounded = false;
            _lastJumpTime = Time.time;
        }
    }

    private void HandleMouseLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked || Mouse.current == null) return;

        Vector2 delta = Mouse.current.delta.ReadValue() * (mouseSensitivity * 0.1f);

        transform.Rotate(Vector3.up * delta.x);

        if (_hasCamera)
        {
            _xRotation -= delta.y;
            _xRotation = Mathf.Clamp(_xRotation, -80f, 80f);
            playerCamera.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        }
    }

    private void FixedUpdate()
    {
        if (Keyboard.current == null) return;

        var kb = Keyboard.current;
        float moveX = 0f;
        float moveZ = 0f;

        if (kb.wKey.isPressed || kb.upArrowKey.isPressed) moveZ += 1f;
        if (kb.sKey.isPressed || kb.downArrowKey.isPressed) moveZ -= 1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) moveX += 1f;
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) moveX -= 1f;

        Vector3 movement = (transform.forward * moveZ + transform.right * moveX).normalized * moveSpeed;
        Vector3 vel = _rb.linearVelocity;
        _rb.linearVelocity = new Vector3(movement.x, vel.y, movement.z);
    }
}