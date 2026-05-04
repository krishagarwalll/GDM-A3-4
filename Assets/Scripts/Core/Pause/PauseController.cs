using UnityEngine;
using UnityEngine.InputSystem;
using Game.Core.Events;

namespace Game.Core.Pause
{
    /// <summary>
    /// Bridges player input + UI channels to the static <see cref="PauseService"/>.
    /// Lives on the persistent managers scene so it survives level reloads.
    ///
    /// - Esc toggles the menu pause.
    /// - lockChannels (e.g. PlayerCaught / PlayerEscaped / TimeUp) disable the menu
    ///   pause until an unlockChannel fires (typically SceneLoaded).
    /// - Show / hide channels drive the visible <see cref="UI.UIPanel"/>.
    /// </summary>
    public class PauseController : MonoBehaviour
    {
        public static PauseController Instance { get; private set; }

        [Header("Hotkey")]
        [SerializeField] private Key toggleKey = Key.Escape;

        [Header("Channels (output)")]
        [SerializeField] private VoidEventChannelSO showPausePanel;
        [SerializeField] private VoidEventChannelSO hidePausePanel;

        [Header("Lockouts")]
        [Tooltip("Raising any of these prevents the player from opening the pause menu (e.g. win/lose triggers).")]
        [SerializeField] private VoidEventChannelSO[] lockChannels;
        [Tooltip("Raising any of these clears the lockout and force-resumes (e.g. SceneLoaded).")]
        [SerializeField] private VoidEventChannelSO[] unlockChannels;

        private bool _locked;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void OnEnable()
        {
            Subscribe(lockChannels, Lock);
            Subscribe(unlockChannels, Unlock);
        }

        private void OnDisable()
        {
            Unsubscribe(lockChannels, Lock);
            Unsubscribe(unlockChannels, Unlock);
        }

        private void Update()
        {
            if (_locked) return;
            var kb = Keyboard.current;
            if (kb == null) return;
            if (kb[toggleKey].wasPressedThisFrame) Toggle();
        }

        public void Toggle()
        {
            if (PauseService.IsPausedFor(PauseReason.Menu)) Resume();
            else Pause();
        }

        public void Pause()
        {
            if (_locked) return;
            if (PauseService.IsPausedFor(PauseReason.Menu)) return;

            PauseService.Pause(PauseReason.Menu);
            showPausePanel?.Raise();
        }

        public void Resume()
        {
            if (!PauseService.IsPausedFor(PauseReason.Menu)) return;

            PauseService.Resume(PauseReason.Menu);
            hidePausePanel?.Raise();
        }

        private void Lock()
        {
            _locked = true;
            if (PauseService.IsPausedFor(PauseReason.Menu)) Resume();
        }

        private void Unlock()
        {
            _locked = false;
            // Safety net — if a scene transition happens while paused for any reason, clear it.
            if (PauseService.IsPaused)
            {
                PauseService.ResumeAll();
                hidePausePanel?.Raise();
            }
        }

        private static void Subscribe(VoidEventChannelSO[] channels, UnityEngine.Events.UnityAction handler)
        {
            if (channels == null) return;
            foreach (var c in channels) if (c != null) c.OnEventRaised += handler;
        }

        private static void Unsubscribe(VoidEventChannelSO[] channels, UnityEngine.Events.UnityAction handler)
        {
            if (channels == null) return;
            foreach (var c in channels) if (c != null) c.OnEventRaised -= handler;
        }
    }
}
