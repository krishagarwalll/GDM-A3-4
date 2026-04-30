using UnityEngine;
using Game.Core.Events;

[DisallowMultipleComponent]
public class PaintingInteractable : MonoBehaviour, IInteractable
{
    [Header("Text")]
    [SerializeField] private string interactText = "Steal painting";
    [SerializeField] private string stolenText = "Already taken";

    [Header("Channels (output)")]
    [SerializeField] private VoidEventChannelSO onStolen;

    [Header("Visuals")]
    [SerializeField] private GameObject hideOnSteal;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator animator;
    [SerializeField] private string stolenTriggerName = "Stolen";
    [SerializeField] private Collider2D triggerCollider;

    private bool _stolen;

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (triggerCollider == null) triggerCollider = GetComponent<Collider2D>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    public void Interact()
    {
        if (_stolen) return;
        _stolen = true;

        if (animator != null && !string.IsNullOrWhiteSpace(stolenTriggerName))
            animator.SetTrigger(stolenTriggerName);
        else if (hideOnSteal != null)
            hideOnSteal.SetActive(false);
        else if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        if (triggerCollider != null) triggerCollider.enabled = false;

        onStolen?.Raise();
    }

    public string GetInteractiveText() => _stolen ? stolenText : interactText;
}
