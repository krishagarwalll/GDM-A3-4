using System.Collections;
using UnityEngine;
using Game.Core.Events;

[DisallowMultipleComponent]
public class SlotsInteractable : MonoBehaviour, IInteractable
{
    [Header("Text")]
    [SerializeField] private string interactText = "Sabotage Slot";
    [SerializeField] private string inProgressText = "Sabotaging...";
    [SerializeField] private string sabotagedText = "Already sabotaged";

    [Header("Channels (output)")]
    [SerializeField] private VoidEventChannelSO onSabotaged;

    [Header("Cooldown Animation")]
    [SerializeField] private GameObject cooldownObject;
    [SerializeField] private Animator animator;
    [SerializeField] private string cooldownTriggerName = "Cooldown";
    [SerializeField] private float cooldownSeconds = 2f;
    [SerializeField] private bool hideCooldownObjectOnStart = true;
    [SerializeField] private bool hideCooldownObjectWhenDone = true;
    [SerializeField] private Collider2D triggerCollider;

    private bool _sabotageInProgress;
    private bool _sabotaged;

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (cooldownObject == null && animator != null) cooldownObject = animator.gameObject;
        if (triggerCollider == null) triggerCollider = GetComponent<Collider2D>();

        if (hideCooldownObjectOnStart && cooldownObject != null)
            cooldownObject.SetActive(false);
    }

    public void Interact()
    {
        if (_sabotaged || _sabotageInProgress) return;
        StartCoroutine(SabotageRoutine());
    }

    private IEnumerator SabotageRoutine()
    {
        _sabotageInProgress = true;
        LockPlayerMovement(true);

        if (cooldownObject != null && !cooldownObject.activeSelf)
            cooldownObject.SetActive(true);

        if (animator != null && !string.IsNullOrWhiteSpace(cooldownTriggerName))
            animator.SetTrigger(cooldownTriggerName);

        yield return WaitForCooldownToFinish();

        _sabotaged = true;
        _sabotageInProgress = false;
        if (triggerCollider != null) triggerCollider.enabled = false;
        if (hideCooldownObjectWhenDone && cooldownObject != null)
            cooldownObject.SetActive(false);
        LockPlayerMovement(false);

        onSabotaged?.Raise();
    }

    private IEnumerator WaitForCooldownToFinish()
    {
        yield return new WaitForSeconds(Mathf.Max(0.1f, cooldownSeconds));
    }

    private void LockPlayerMovement(bool shouldLock)
    {
        if (Player.Instance == null) return;
        Player.Instance.SetMovementLocked(shouldLock);
    }

    public string GetInteractiveText()
    {
        if (_sabotaged) return sabotagedText;
        if (_sabotageInProgress) return inProgressText;
        return interactText;
    }
}
