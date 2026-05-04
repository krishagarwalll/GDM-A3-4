using System;
using UnityEngine;

namespace Game.Core.Pause
{
    /// <summary>
    /// Global, reason-stacked pause service.
    ///
    /// - Multiple callers can pause for independent reasons; the game stays paused
    ///   until every reason is released.
    /// - Drives Time.timeScale to 0 while paused so physics, animators, particle
    ///   systems and Time.deltaTime-driven logic freeze automatically.
    /// - AudioSources are unaffected by Time.timeScale, so music continues. Do not
    ///   set AudioListener.pause anywhere.
    /// - Static state is reset on subsystem registration so it survives a domain
    ///   reload cleanly between Play sessions.
    /// </summary>
    public static class PauseService
    {
        private static PauseReason _reasons;
        private static float _timeScaleBeforePause = 1f;

        public static PauseReason ActiveReasons => _reasons;
        public static bool IsPaused => _reasons != PauseReason.None;
        public static bool IsPausedFor(PauseReason reason) => (_reasons & reason) != 0;

        /// <summary>Fired whenever the active reason set changes while paused.</summary>
        public static event Action<PauseReason> Paused;

        /// <summary>Fired when the last reason is released and the game resumes.</summary>
        public static event Action<PauseReason> Resumed;

        public static void Pause(PauseReason reason)
        {
            if (reason == PauseReason.None) return;

            bool wasPaused = IsPaused;
            _reasons |= reason;

            if (!wasPaused)
            {
                _timeScaleBeforePause = Time.timeScale > 0f ? Time.timeScale : 1f;
                Time.timeScale = 0f;
            }

            Paused?.Invoke(_reasons);
        }

        public static void Resume(PauseReason reason)
        {
            if (reason == PauseReason.None || !IsPaused) return;

            _reasons &= ~reason;

            if (!IsPaused)
            {
                Time.timeScale = _timeScaleBeforePause;
                Resumed?.Invoke(reason);
            }
        }

        /// <summary>Force-clear every pause reason. Use on scene transitions.</summary>
        public static void ResumeAll()
        {
            if (!IsPaused) return;
            var prev = _reasons;
            _reasons = PauseReason.None;
            Time.timeScale = _timeScaleBeforePause;
            Resumed?.Invoke(prev);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _reasons = PauseReason.None;
            _timeScaleBeforePause = 1f;
            Paused = null;
            Resumed = null;
        }
    }
}
