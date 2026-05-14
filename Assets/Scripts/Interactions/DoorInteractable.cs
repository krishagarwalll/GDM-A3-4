using UnityEngine;

[DisallowMultipleComponent]
public class DoorInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactText = "Unlock Door";
    [SerializeField] private string missingKeyText = "Needs a Key";

    public void Interact()
    {
        if (!KeyInteractable.TryUseKey())
            return;
        
        Destroy(gameObject);
    }

    public string GetInteractiveText()
    {
        return KeyInteractable.HasKey()  ? interactText : missingKeyText;
    }
}