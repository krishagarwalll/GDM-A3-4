using UnityEngine;
using Game.Core.Events;

[DisallowMultipleComponent]
public class ChestInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private Animator animator;
    [SerializeField] private string openTriggerName = "Open";
    [SerializeField] private string interactText = "Open chest";
    [SerializeField] private string openedText = "Chest is open";
    [SerializeField] private string missingKeyText = "Need a key";
    [SerializeField] private bool consumeKeyOnOpen = true;

    [Header("Channels (output)")]
    [SerializeField] private VoidEventChannelSO onOpened;

    private bool isOpen;

    private void Awake()
    {
        if (!animator)
            animator = GetComponent<Animator>();
    }

    public void Interact()
    {
        if (isOpen)
            return;

        if (consumeKeyOnOpen)
        {
            if (!KeyInteractable.TryUseKey())
                return;
        }
        else if (!KeyInteractable.HasKey())
        {
            return;
        }

        isOpen = true;

        if (animator && !string.IsNullOrWhiteSpace(openTriggerName))
            animator.SetTrigger(openTriggerName);

        onOpened?.Raise();
    }

    public string GetInteractiveText()
    {
        if (isOpen)
            return openedText;

        return KeyInteractable.HasKey() ? interactText : missingKeyText;
    }
}
