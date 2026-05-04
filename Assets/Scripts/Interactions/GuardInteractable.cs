using UnityEngine;
using Game.Core.Events;

[DisallowMultipleComponent]
public class GuardInteractable : MonoBehaviour, IInteractable
{
    [Header("Text")]
    [SerializeField] private string interactText = "Take down guard";
    [SerializeField] private string downText = "";
    [SerializeField] private string requiresCrouchText = "Crouch to take down";

    [Header("Behaviour")]
    [SerializeField] private GameObject hideOnTakedown;
    [SerializeField] private bool requireCrouch = true;
    [SerializeField] private EnemyAI enemyAI;

    [Header("Channels (output)")]
    [SerializeField] private VoidEventChannelSO onTakenDown;

    private bool _down;

    private void Awake() {
        if (enemyAI == null) enemyAI = GetComponentInParent<EnemyAI>();
    }

    public void Interact()
    {
        if (_down) return;
        if (!CanTakeDown()) return;

        _down = true;

        if (enemyAI != null) enemyAI.KnockOut();
        onTakenDown?.Raise();

        var target = hideOnTakedown != null ? hideOnTakedown : gameObject;
        target.SetActive(false);
    }

    public string GetInteractiveText()
    {
        if (_down) return downText;
        if (!CanTakeDown()) return requiresCrouchText;
        return interactText;
    }

    private bool CanTakeDown()
    {
        if (!requireCrouch) return true;
        if (Player.Instance == null) return false;
        return Player.Instance.IsCrouching;
    }
}
