using UnityEngine;
using TMPro;
using Game.Core.Events;
using Game.Core.Game;

namespace Game.UI
{
    public class ObjectiveCounterDisplay : MonoBehaviour
    {
        [Header("Channels (input)")]
        [SerializeField] private IntEventChannelSO onCountChanged;

        [Header("Data")]
        [SerializeField] private LevelDataSO levelData;

        [Header("UI")]
        [SerializeField] private TextMeshProUGUI label;

        private void Awake()
        {
            if (label == null) label = GetComponent<TextMeshProUGUI>();
            UpdateLabel(0);
        }

        private void OnEnable()
        {
            if (onCountChanged != null) onCountChanged.OnEventRaised += UpdateLabel;
        }

        private void OnDisable()
        {
            if (onCountChanged != null) onCountChanged.OnEventRaised -= UpdateLabel;
        }

        private void UpdateLabel(int current)
        {
            if (label == null || levelData == null) return;
            label.text = $"{levelData.objectiveLabel}: {current} / {levelData.objectiveCount}";
        }
    }
}
