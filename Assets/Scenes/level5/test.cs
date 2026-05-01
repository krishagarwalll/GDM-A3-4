using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpotLight : MonoBehaviour
{
    Vector2 lastDir = Vector2.down;

    void Update()
    {
        Vector2 input = Vector2.zero;

        // ÐÂ°æ Input System Ð´·¨
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            input.y = 1;
        else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            input.y = -1;

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            input.x = 1;
        else if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            input.x = -1;

        if (input != Vector2.zero)
            lastDir = input;

        float angle = Mathf.Atan2(lastDir.y, lastDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }
}