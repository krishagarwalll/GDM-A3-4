using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour {

    public static Player Instance { get; private set; }

    private const float MOVE_SPEED = 6f;
    private const float CROUCH_SPEED_MULTIPLIER = 0.5f;

    [Header("Crouch")]
    [SerializeField] private Key crouchToggleKey = Key.C;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector3 moveDir;
    private bool isCrouching;

    public bool IsCrouching => isCrouching;
    public bool IsMoving => moveDir.sqrMagnitude > 0;
    public Vector3 GetPosition => transform.position;

    private void Awake() {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void OnDestroy() {
        if (Instance == this) Instance = null;
    }

    private void Update() {
        HandleCrouchToggle();

        float moveX = 0f;
        float moveY = 0f;

        if (Keyboard.current.wKey.isPressed) moveY = +1f;
        if (Keyboard.current.sKey.isPressed) moveY = -1f;
        if (Keyboard.current.aKey.isPressed) moveX = -1f;
        if (Keyboard.current.dKey.isPressed) moveX = +1f;

        moveDir = new Vector3(moveX, moveY).normalized;

        bool isMoving = moveDir.sqrMagnitude > 0;
        animator.SetBool("isRunning", isMoving);

        if (moveX > 0) {
            transform.localScale = new Vector3(1, 1, 1);
        } else if (moveX < 0) {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private void FixedUpdate() {
        float speed = MOVE_SPEED * (isCrouching ? CROUCH_SPEED_MULTIPLIER : 1f);
        rb.linearVelocity = moveDir * speed;
    }

    private void HandleCrouchToggle() {
        if (Keyboard.current == null) return;
        if (Keyboard.current[crouchToggleKey].wasPressedThisFrame) {
            isCrouching = !isCrouching;
            if (animator != null && HasAnimatorParameter("isCrouching")) {
                animator.SetBool("isCrouching", isCrouching);
            }
        }
    }

    private bool HasAnimatorParameter(string paramName) {
        var ps = animator.parameters;
        for (int i = 0; i < ps.Length; i++) {
            if (ps[i].name == paramName) return true;
        }
        return false;
    }
}
