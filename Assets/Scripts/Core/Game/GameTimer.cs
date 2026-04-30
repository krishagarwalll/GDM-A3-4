using UnityEngine;
using Game.Core.Events;

namespace Game.Core.Game
{
    public class GameTimer : MonoBehaviour
    {
        [Header("Config")]
        public float durationSeconds = 120f;
        [SerializeField] private bool autoStart = true;

        [Header("Channels (output)")]
        [SerializeField] private FloatEventChannelSO onTick;
        [SerializeField] private VoidEventChannelSO onTimeUp;

        public float TimeRemaining { get; private set; }
        public bool IsRunning { get; private set; }

        private void Start()
        {
            TimeRemaining = durationSeconds;
            onTick?.Raise(TimeRemaining);
            if (autoStart) StartTimer();
        }

        public void StartTimer() { IsRunning = true; }
        public void StopTimer() { IsRunning = false; }

        public void Reset(float duration)
        {
            durationSeconds = duration;
            TimeRemaining = duration;
            onTick?.Raise(TimeRemaining);
        }

        private void Update()
        {
            if (!IsRunning) return;

            TimeRemaining -= Time.deltaTime;
            if (TimeRemaining <= 0f)
            {
                TimeRemaining = 0f;
                IsRunning = false;
                onTick?.Raise(0f);
                onTimeUp?.Raise();
                return;
            }
            onTick?.Raise(TimeRemaining);
        }
    }
}
