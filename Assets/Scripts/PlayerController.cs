using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Налаштування руху")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float rotationSpeed = 100f;

    [Header("Перевірка підлоги (Ground Check)")]
    public float groundCheckRadius = 0.25f;

    private Rigidbody rb;
    private CapsuleCollider col;
    private bool isGrounded;
    private float lastJumpTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();

        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void Update()
    {
        if (Time.time - lastJumpTime > 0.15f && rb.linearVelocity.y <= 0.1f)
        {
            Vector3 spherePos = transform.position + Vector3.down * (col.height * 0.5f - groundCheckRadius + 0.05f);
            Collider[] colliders = Physics.OverlapSphere(spherePos, groundCheckRadius);

            isGrounded = false;
            foreach (var hit in colliders)
            {
                if (hit != col && !hit.isTrigger)
                {
                    isGrounded = true;
                    break;
                }
            }
        }
        else
        {
            isGrounded = false;
        }

        bool jump = false;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed)
            jump = true;
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKey(KeyCode.Space))
            jump = true;
#endif

        if (jump && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
            lastJumpTime = Time.time;
        }
    }

    private void FixedUpdate()
    {
        float moveX = 0f;
        float moveZ = 0f;
        float turn = 0f;

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null)
        {
            var kb = Keyboard.current;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) moveZ += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) moveZ -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) moveX += 1f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) moveX -= 1f;

            if (kb.qKey.isPressed) turn -= 1f;
            if (kb.eKey.isPressed) turn += 1f;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (moveX == 0f && moveZ == 0f)
        {
            moveX = Input.GetAxis("Horizontal");
            moveZ = Input.GetAxis("Vertical");
        }
        if (turn == 0f)
        {
            if (Input.GetKey(KeyCode.Q)) turn -= 1f;
            if (Input.GetKey(KeyCode.E)) turn += 1f;
        }
#endif

        Vector3 movement = (transform.forward * moveZ + transform.right * moveX).normalized * moveSpeed;

        Vector3 vel = rb.linearVelocity;
        rb.linearVelocity = new Vector3(movement.x, vel.y, movement.z);

        if (turn != 0f)
        {
            transform.Rotate(Vector3.up, turn * rotationSpeed * Time.fixedDeltaTime);
        }
    }
}