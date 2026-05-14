using UnityEngine;
using Game.Core.Events;

[DisallowMultipleComponent]
public class ChestInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private Animator animator;
    [SerializeField] private string openTriggerName = "Open";
    [SerializeField] private string lootTriggerName = "Loot";
    [SerializeField] private string interactText = "Open chest";
    [SerializeField] private string lootText = "Loot chest";
    [SerializeField] private string lootedText = "Empty";
    [SerializeField] private string missingKeyText = "Need a key";
    [SerializeField] private bool consumeKeyOnOpen = true;

    [Header("Channels (output)")]
    [SerializeField] private VoidEventChannelSO onOpened;
    [SerializeField] private VoidEventChannelSO onLooted;

    private bool isOpen;
    private bool isLooted;

    private void Awake()
    {
        if (!animator)
            animator = GetComponent<Animator>();
    }

    public void Interact()
    {
        if (isLooted)
            return;

        if (!isOpen)
        {
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
            return;
        }

        isLooted = true;

        if (animator && !string.IsNullOrWhiteSpace(lootTriggerName))
            animator.SetTrigger(lootTriggerName);

        onLooted?.Raise();
    }

    public string GetInteractiveText()
    {
        if (isLooted)
            return lootedText;

        if (isOpen)
            return lootText;

        return KeyInteractable.HasKey() ? interactText : missingKeyText;
    }
}
