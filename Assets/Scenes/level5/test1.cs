using UnityEngine;
using Game.Core.Events;

[DisallowMultipleComponent]
public class CollectibleItem : MonoBehaviour, IInteractable
{
    [Header("Text")]
    [SerializeField] private string interactText = "Press E to collect";
    [SerializeField] private string collectedText = "Already collected";

    [Header("Channels (output)")]
    [SerializeField] private VoidEventChannelSO onCollected;

    private bool _collected;

    public void Interact()
    {
        if (_collected) return;
        _collected = true;

        onCollected?.Raise();
        gameObject.SetActive(false);
    }

    public string GetInteractiveText() => _collected ? collectedText : interactText;
}
