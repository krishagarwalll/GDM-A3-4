using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightTurnOff : MonoBehaviour, IInteractable
{
    [SerializeField] Light2D pointLight;

    public string GetInteractiveText()
    {
        return pointLight.enabled ? "Press E Turn on" : "Press E Turn off";
    }

    public void Interact()
    {
        pointLight.enabled = !pointLight.enabled;
    }
}