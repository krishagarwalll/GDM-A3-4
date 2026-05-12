using UnityEngine;

public enum LockpickResultType
{
    DestroyDoor,
    RevealKey
}

[DisallowMultipleComponent]
public class LockpickInteractable : MonoBehaviour, IInteractable
{
    [Header("Settings")]
    [SerializeField] private LockpickResultType resultType;
    [SerializeField] private string interactText = "Pick Lock";
    
    [Header("Reveal Key (only if Result Type is Reveal Key")]
    [SerializeField] private GameObject keyToReveal;
    
    private bool BeenUsed;

    public void Interact()
    {
        if (BeenUsed) return;
        LockpickManager.Instance.OpenLockpick(OnSuccess);
    }

    private void OnSuccess()
    {
        BeenUsed = true;
        switch (resultType)
        {
            case LockpickResultType.DestroyDoor:
                Destroy(gameObject);
                break;
            
            case LockpickResultType.RevealKey:
                if (keyToReveal != null)
                    keyToReveal.SetActive(true);
                break;
        }
    }

    public string GetInteractiveText()
    {
        if (BeenUsed) return "";
        return interactText;
    }
}
