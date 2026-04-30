using UnityEngine;
using Game.Core.Events;

namespace Game.UI
{
    public class UIPanel : MonoBehaviour
    {
        [Header("Channels (input)")]
        [SerializeField] private VoidEventChannelSO showChannel;
        [SerializeField] private VoidEventChannelSO hideChannel;

        [Header("Behaviour")]
        [SerializeField] private bool startVisible = false;
        [SerializeField] private bool pauseGameWhileVisible = false;

        private bool _isVisible;

        private void Awake()
        {
            ApplyVisibility(startVisible);
        }

        private void OnEnable()
        {
            if (showChannel != null) showChannel.OnEventRaised += Show;
            if (hideChannel != null) hideChannel.OnEventRaised += Hide;
        }

        private void OnDisable()
        {
            if (showChannel != null) showChannel.OnEventRaised -= Show;
            if (hideChannel != null) hideChannel.OnEventRaised -= Hide;
            if (pauseGameWhileVisible && _isVisible)
                Time.timeScale = 1f;
        }

        public void Show() => ApplyVisibility(true);
        public void Hide() => ApplyVisibility(false);
        public void Toggle() => ApplyVisibility(!_isVisible);

        private void ApplyVisibility(bool visible)
        {
            _isVisible = visible;
            if (gameObject.activeSelf != visible)
                gameObject.SetActive(visible);

            if (pauseGameWhileVisible)
                Time.timeScale = visible ? 0f : 1f;
        }
    }
}
