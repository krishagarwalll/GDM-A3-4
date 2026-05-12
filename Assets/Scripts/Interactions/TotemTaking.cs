using UnityEngine;
using Game.Core.Events;

[DisallowMultipleComponent]
public class TotemInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactText = "Take Totem";
    [SerializeField] private string takenText = "Already acquired";

    [SerializeField] private VoidEventChannelSO onTaken;

    [SerializeField] private GameObject visualObject;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Collider2D interactionCollider;

    private bool _taken;

    private void Awake()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (interactionCollider == null) interactionCollider = GetComponent<Collider2D>();
    }

    public void Interact()
    {
        if (_taken) return;
        _taken = true;

        if (visualObject != null)
            visualObject.SetActive(false);
        else if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        if (interactionCollider != null)
            interactionCollider.enabled = false;

        onTaken?.Raise();
    }

    public string GetInteractiveText() => _taken ? takenText : interactText;
}