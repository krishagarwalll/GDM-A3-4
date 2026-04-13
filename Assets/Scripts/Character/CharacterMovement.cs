using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMovement : MonoBehaviour {

    private const float MOVE_SPEED = 6f;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector3 moveDir;
    
    private void Awake() {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update() {
        float moveX = 0f;
        float moveY = 0f;

        if (Keyboard.current.wKey.isPressed) moveY = +1f;
        if (Keyboard.current.sKey.isPressed) moveY = -1f;
        if (Keyboard.current.aKey.isPressed) moveX = -1f;
        if (Keyboard.current.dKey.isPressed) moveX = +1f;

        moveDir = new Vector3(moveX, moveY).normalized;

        bool isMoving = moveDir.sqrMagnitude > 0;
        animator.SetBool("isRunning", isMoving);

        if (moveX > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
        else if (moveX < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    private void FixedUpdate() {
        rb.linearVelocity = moveDir * MOVE_SPEED;
    }

}
