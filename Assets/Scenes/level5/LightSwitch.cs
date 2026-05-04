using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightSwitch : MonoBehaviour, IInteractable
{
    [SerializeField] Light2D globalLight;
    [SerializeField] Light2D playerSpotLight;
    [SerializeField] float darkIntensity = 0.002f;
    [SerializeField] float normalIntensity = 1f;

    void Start()
    {
        globalLight.intensity = darkIntensity;
        playerSpotLight.enabled = true;
    }

    public string GetInteractiveText()
    {
        bool lightOn = Level5GameController.Instance != null && Level5GameController.Instance.IsLightOn;
        return lightOn ? "Press E turn off" : "Press E turn on";
    }

    public void Interact()
    {
        bool newState = Level5GameController.Instance == null || !Level5GameController.Instance.IsLightOn;

        if (newState)
        {
            globalLight.intensity = normalIntensity;
            playerSpotLight.enabled = false;
        }
        else
        {
            globalLight.intensity = darkIntensity;
            playerSpotLight.enabled = true;
        }

        Level5GameController.Instance?.SetLightState(newState);
    }
}
