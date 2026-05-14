using UnityEngine;
using Game.Core.Events;
using Game.Core.Game;

[DisallowMultipleComponent]
public class ExitInteractable : MonoBehaviour, IInteractable
{
    [Header("Refs")]
    [SerializeField] private ObjectiveTracker objectiveTracker;
    [SerializeField] private LevelDataSO levelData;

    [Header("Text")]
    [SerializeField] private string readyText = "Escape!";
    [SerializeField] private string blockedTextFormat = "Need {0}/{1} first";

    [Header("Channels (output)")]
    [SerializeField] private VoidEventChannelSO onEscaped;

    private bool _escaped;

    public void Interact()
    {
        if (_escaped) return;
        if (objectiveTracker == null || !objectiveTracker.IsComplete) return;
        _escaped = true;
        onEscaped?.Raise();
    }

    public string GetInteractiveText()
    {
        if (objectiveTracker == null) return readyText;
        if (objectiveTracker.IsComplete) return readyText;
        int cur = objectiveTracker.CurrentCount;
        int max = objectiveTracker.requiredCount;
        return string.Format(blockedTextFormat, cur, max);
    }
}
