using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CinemachineCamera))]
public class CameraPeek : MonoBehaviour
{
    [SerializeField] private float peekDistance = 1.75f;
    [SerializeField] private float peekSmoothSpeed = 8f;

    private CinemachinePositionComposer positionComposer;
    private Vector3 currentOffset;

    private void Awake()
    {
        positionComposer = GetComponent<CinemachinePositionComposer>();
        if (positionComposer == null)
        {
            positionComposer = GetComponentInChildren<CinemachinePositionComposer>();
        }
    }

    private void Update()
    {
        if (positionComposer == null || Keyboard.current == null)
        {
            return;
        }

        Vector2 peekInput = ReadPeekInput();
        Vector3 targetOffset = new Vector3(peekInput.x, peekInput.y, 0f) * peekDistance;

        currentOffset = Vector3.Lerp(
            currentOffset,
            targetOffset,
            1f - Mathf.Exp(-peekSmoothSpeed * Time.deltaTime)
        );

        positionComposer.TargetOffset = currentOffset;
    }

    private static Vector2 ReadPeekInput()
    {
        float x = 0f;
        float y = 0f;

        if (Keyboard.current.leftArrowKey.isPressed) x -= 1f;
        if (Keyboard.current.rightArrowKey.isPressed) x += 1f;
        if (Keyboard.current.downArrowKey.isPressed) y -= 1f;
        if (Keyboard.current.upArrowKey.isPressed) y += 1f;

        return new Vector2(x, y).normalized;
    }
}
