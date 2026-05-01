using UnityEngine;

public class HudTrigger : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject hudObject;
    private bool isVisible = false;

    public string GetInteractiveText()
    {
        return isVisible ? "按 E 关闭 HUD" : "按 E 打开 HUD";
    }

    public void Interact()
    {
        isVisible = !isVisible;
        SetHudActive(hudObject, isVisible);
    }

    private void SetHudActive(GameObject obj, bool active)
    {
        obj.SetActive(active);
        foreach (Transform child in obj.transform)
        {
            SetHudActive(child.gameObject, active);
        }
    }
}