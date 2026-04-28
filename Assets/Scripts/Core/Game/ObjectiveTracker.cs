using UnityEngine;
using Game.Core.Events;

namespace Game.Core.Game
{
    public class ObjectiveTracker : MonoBehaviour
    {
        [Header("Config")]
        public int requiredCount = 1;

        [Header("Channels (input)")]
        [SerializeField] private VoidEventChannelSO onObjectiveProgress;

        [Header("Channels (output)")]
        [SerializeField] private IntEventChannelSO onCountChanged;
        [SerializeField] private VoidEventChannelSO onComplete;

        public int CurrentCount { get; private set; }
        public bool IsComplete => CurrentCount >= requiredCount;

        private void OnEnable()
        {
            if (onObjectiveProgress != null)
                onObjectiveProgress.OnEventRaised += HandleProgress;
        }

        private void OnDisable()
        {
            if (onObjectiveProgress != null)
                onObjectiveProgress.OnEventRaised -= HandleProgress;
        }

        private void Start()
        {
            onCountChanged?.Raise(CurrentCount);
        }

        private void HandleProgress()
        {
            if (IsComplete) return;
            CurrentCount++;
            onCountChanged?.Raise(CurrentCount);
            if (IsComplete)
                onComplete?.Raise();
        }
    }
}
