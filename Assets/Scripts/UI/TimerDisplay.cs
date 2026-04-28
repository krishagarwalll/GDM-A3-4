using UnityEngine;
using TMPro;
using Game.Core.Events;

namespace Game.UI
{
    public class TimerDisplay : MonoBehaviour
    {
        [Header("Channels (input)")]
        [SerializeField] private FloatEventChannelSO onTimerTick;

        [Header("UI")]
        [SerializeField] private TextMeshProUGUI label;

        [Header("Warning state")]
        [SerializeField] private float warnAtSeconds = 30f;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color warnColor = new Color(1f, 0.35f, 0.35f, 1f);

        private void Awake()
        {
            if (label == null) label = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            if (onTimerTick != null) onTimerTick.OnEventRaised += HandleTick;
        }

        private void OnDisable()
        {
            if (onTimerTick != null) onTimerTick.OnEventRaised -= HandleTick;
        }

        private void HandleTick(float secondsRemaining)
        {
            if (label == null) return;
            int total = Mathf.CeilToInt(secondsRemaining);
            int minutes = total / 60;
            int seconds = total % 60;
            label.text = $"{minutes:0}:{seconds:00}";
            label.color = secondsRemaining <= warnAtSeconds ? warnColor : normalColor;
        }
    }
}
