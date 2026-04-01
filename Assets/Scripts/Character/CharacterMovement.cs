using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterMovement : MonoBehaviour {

    private const float MOVE_SPEED = 6f;

    private Rigidbody2D rigidbody2D;
    private Vector3 moveDir;
    private bool isDashButtonDown;

    private void Awake() {
        rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void Update() {
        float moveX = 0f;
        float moveY = 0f;

        if (Keyboard.current.wKey.isPressed) {
            moveY = +1f;
        }
        if (Keyboard.current.sKey.isPressed) {
            moveY = -1f;
        }
        if (Keyboard.current.aKey.isPressed) {
            moveX = -1f;
        }
        if (Keyboard.current.dKey.isPressed) {
            moveX = +1f;
        }

        moveDir = new Vector3(moveX, moveY).normalized;
        //Add Animation support
        //.PlayMoveAnim(moveDir);
    }

    private void FixedUpdate() {
        rigidbody2D.linearVelocity = moveDir * MOVE_SPEED;
    }

}