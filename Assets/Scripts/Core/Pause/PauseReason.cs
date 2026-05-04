using System;

namespace Game.Core.Pause
{
    /// <summary>
    /// Bit-flag set of reasons the game can be paused.
    /// Multiple subsystems can request pause independently; the game only resumes
    /// once every requesting subsystem releases its reason.
    /// </summary>
    [Flags]
    public enum PauseReason
    {
        None         = 0,
        Menu         = 1 << 0, // Player-opened pause menu (Esc)
        LevelOutcome = 1 << 1, // Win / lose panel
        Cutscene     = 1 << 2,
        Dialogue     = 1 << 3,
        Settings     = 1 << 4,
        Debug        = 1 << 5,
        All          = ~0,
    }
}
