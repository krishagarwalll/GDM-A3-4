using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightSwitch : MonoBehaviour, IInteractable
{
    [SerializeField] Light2D globalLight;
    [SerializeField] Light2D playerSpotLight;
    [SerializeField] float darkIntensity = 0.002f;
    [SerializeField] float normalIntensity = 1f;

    bool isLimitMode = true; 

    void Start()
    {
        globalLight.intensity = darkIntensity;
        playerSpotLight.enabled = true;
    }

    public string GetInteractiveText()
    {
        return isLimitMode ? "Press E turn on" : "Press E turn off";
    }

    public void Interact()
    {
        isLimitMode = !isLimitMode;

        if (isLimitMode)
        {
            globalLight.intensity = darkIntensity;
            playerSpotLight.enabled = true;
        }
        else
        {
            globalLight.intensity = normalIntensity;
            playerSpotLight.enabled = false;
        }
    }
}