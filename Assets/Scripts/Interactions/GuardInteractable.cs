using UnityEngine;
using Game.Core.Events;

[DisallowMultipleComponent]
public class GuardInteractable : MonoBehaviour, IInteractable
{
    [Header("Text")]
    [SerializeField] private string interactText = "Take down guard";
    [SerializeField] private string downText = "";

    [Header("Behaviour")]
    [SerializeField] private GameObject hideOnTakedown;

    [Header("Channels (output)")]
    [SerializeField] private VoidEventChannelSO onTakenDown;

    private bool _down;

    public void Interact()
    {
        if (_down) return;
        _down = true;

        onTakenDown?.Raise();

        var target = hideOnTakedown != null ? hideOnTakedown : gameObject;
        target.SetActive(false);
    }

    public string GetInteractiveText() => _down ? downText : interactText;
}
