using System;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using Game.Core.Events;

namespace Game.UI
{
    // Binds one shared UIPanel to multiple outcome triggers (e.g. win / lose).
    // On a trigger fires: writes the title, shows only the listed buttons,
    // then calls panel.Show(). Lives on an always-active GameObject so it
    // can subscribe regardless of the panel's active state.
    public class LevelOutcomeBinder : MonoBehaviour
    {
        [Serializable]
        public class Outcome
        {
            public VoidEventChannelSO trigger;
            [TextArea] public string title;
            public GameObject[] buttonsToShow;
        }

        [Header("Refs")]
        [SerializeField] private UIPanel panel;
        [SerializeField] private TextMeshProUGUI titleLabel;
        [Tooltip("Every outcome button across the panel. Buttons not in an outcome's buttonsToShow get hidden.")]
        [SerializeField] private GameObject[] allOutcomeButtons;

        [Header("Outcomes")]
        [SerializeField] private Outcome[] outcomes;

        private UnityAction[] _handlers;

        private void Awake()
        {
            if (outcomes == null) return;
            _handlers = new UnityAction[outcomes.Length];
            for (int i = 0; i < outcomes.Length; i++)
            {
                var captured = outcomes[i];
                if (captured == null) continue;
                _handlers[i] = () => Apply(captured);
                if (captured.trigger != null)
                    captured.trigger.OnEventRaised += _handlers[i];
            }
        }

        private void OnDestroy()
        {
            if (outcomes == null || _handlers == null) return;
            for (int i = 0; i < outcomes.Length; i++)
            {
                if (outcomes[i]?.trigger != null && _handlers[i] != null)
                    outcomes[i].trigger.OnEventRaised -= _handlers[i];
            }
        }

        private void Apply(Outcome o)
        {
            if (titleLabel != null) titleLabel.text = o.title;

            if (allOutcomeButtons != null)
            {
                foreach (var b in allOutcomeButtons)
                    if (b != null) b.SetActive(false);
            }
            if (o.buttonsToShow != null)
            {
                foreach (var b in o.buttonsToShow)
                    if (b != null) b.SetActive(true);
            }

            if (panel != null) panel.Show();
        }
    }
}
