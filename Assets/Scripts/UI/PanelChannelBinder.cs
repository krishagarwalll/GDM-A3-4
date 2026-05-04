using UnityEngine;
using Game.Core.Events;

namespace Game.UI
{
    /// <summary>
    /// Bridges show / hide channels to a <see cref="UIPanel"/> from an
    /// always-active GameObject, so the panel itself can start disabled
    /// (its OnEnable would never fire to subscribe directly).
    /// </summary>
    public class PanelChannelBinder : MonoBehaviour
    {
        [SerializeField] private UIPanel panel;
        [SerializeField] private VoidEventChannelSO showChannel;
        [SerializeField] private VoidEventChannelSO hideChannel;

        private void OnEnable()
        {
            if (showChannel != null) showChannel.OnEventRaised += ShowPanel;
            if (hideChannel != null) hideChannel.OnEventRaised += HidePanel;
        }

        private void OnDisable()
        {
            if (showChannel != null) showChannel.OnEventRaised -= ShowPanel;
            if (hideChannel != null) hideChannel.OnEventRaised -= HidePanel;
        }

        private void ShowPanel() { if (panel != null) panel.Show(); }
        private void HidePanel() { if (panel != null) panel.Hide(); }
    }
}
