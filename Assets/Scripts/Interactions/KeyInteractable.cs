using UnityEngine;

[DisallowMultipleComponent]
public class KeyInteractable : MonoBehaviour, IInteractable
{
    [SerializeField, Min(1)] private int keyCount = 1;
    [SerializeField] private string interactText = "Pick up key";

    public static int HeldKeyCount { get; private set; }

    private bool collected;

    public void Interact()
    {
        if (collected)
            return;

        collected = true;
        HeldKeyCount += Mathf.Max(1, keyCount);
        Destroy(gameObject);
    }

    public string GetInteractiveText()
    {
        return interactText;
    }

    public static bool HasKey()
    {
        return HeldKeyCount > 0;
    }

    public static bool TryUseKey()
    {
        if (HeldKeyCount <= 0)
            return false;

        HeldKeyCount--;
        return true;
    }
}
