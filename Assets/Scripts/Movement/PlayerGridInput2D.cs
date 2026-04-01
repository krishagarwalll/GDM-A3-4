using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[RequireComponent(typeof(GridMovement2D))]
public class PlayerGridInput2D : MonoBehaviour
{
    [SerializeField, Min(0f)] private float inputDeadzone = 0.25f;
    [SerializeField, Min(0f)] private float holdRepeatDelay = 0.3f;
    [SerializeField, Min(0.01f)] private float holdRepeatInterval = 0.12f;

    private GridMovement2D gridMovement;
    private Vector2 heldDirection;
    private float nextRepeatTime;

    private GridMovement2D Mover => gridMovement ? gridMovement : gridMovement = GetComponent<GridMovement2D>();

    private void OnDisable()
    {
        if (gridMovement)
            gridMovement.ClearDesiredDirection();

        heldDirection = Vector2.zero;
        nextRepeatTime = 0f;
    }

    private void Update()
    {
        var mover = Mover;
        if (!mover)
            return;

        var rawInput = ReadMoveInput();
        var direction = GridMovement2D.ToCardinal(rawInput, inputDeadzone, mover.CurrentDirection);
        HandleMoveInput(mover, direction);
    }

    private void HandleMoveInput(GridMovement2D mover, Vector2 direction)
    {
        if (direction == Vector2.zero)
        {
            heldDirection = Vector2.zero;
            nextRepeatTime = 0f;
            return;
        }

        if (direction != heldDirection)
        {
            heldDirection = direction;
            nextRepeatTime = Time.time + holdRepeatDelay;
            mover.RequestStep(direction);
            return;
        }

        if (Time.time < nextRepeatTime)
            return;

        mover.RequestStep(direction);
        nextRepeatTime = Time.time + holdRepeatInterval;
    }

    private static Vector2 ReadMoveInput()
    {
        var input = Vector2.zero;

        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                input.x -= 1f;

            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                input.x += 1f;

            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                input.y -= 1f;

            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                input.y += 1f;
        }

        var gamepad = Gamepad.current;
        if (gamepad != null)
        {
            var stickInput = gamepad.leftStick.ReadValue();
            if (stickInput.sqrMagnitude > input.sqrMagnitude)
                input = stickInput;
        }

        return Vector2.ClampMagnitude(input, 1f);
    }
}
