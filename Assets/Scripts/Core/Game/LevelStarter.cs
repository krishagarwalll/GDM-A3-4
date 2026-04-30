using UnityEngine;

namespace Game.Core.Game
{
    [DefaultExecutionOrder(-100)]
    public class LevelStarter : MonoBehaviour
    {
        [Header("Config")]
        public LevelDataSO levelData;

        [Header("Scene refs")]
        [SerializeField] private GameTimer timer;
        [SerializeField] private ObjectiveTracker objectiveTracker;

        public LevelDataSO LevelData => levelData;

        private void Awake()
        {
            Time.timeScale = 1f;

            if (levelData == null)
            {
                Debug.LogError($"[LevelStarter] '{name}' has no LevelData assigned. Timer / tracker will use their default values.");
                return;
            }

            if (timer != null)
                timer.durationSeconds = levelData.durationSeconds;
            if (objectiveTracker != null)
                objectiveTracker.requiredCount = levelData.objectiveCount;
        }
    }
}
