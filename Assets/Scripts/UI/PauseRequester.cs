using UnityEngine;
using Game.Core.Pause;

namespace Game.UI
{
    /// <summary>
    /// Tiny adapter so UI Buttons can pause / resume without holding a direct
    /// reference to the persistent <see cref="PauseController"/>.
    /// </summary>
    public class PauseRequester : MonoBehaviour
    {
        public void Pause()
        {
            if (PauseController.Instance != null) PauseController.Instance.Pause();
        }

        public void Resume()
        {
            if (PauseController.Instance != null) PauseController.Instance.Resume();
        }

        public void Toggle()
        {
            if (PauseController.Instance != null) PauseController.Instance.Toggle();
        }
    }
}
