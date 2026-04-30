using UnityEngine;
using Game.Core.Scenes;

namespace Game.Core.Game
{
    [CreateAssetMenu(menuName = "Game/Game/Level Data", fileName = "LevelData_")]
    public class LevelDataSO : ScriptableObject
    {
        [Header("Identity")]
        public string displayName = "Level";
        public string objectiveLabel = "Paintings";

        [Header("Timing")]
        public float durationSeconds = 120f;

        [Header("Objective")]
        public int objectiveCount = 3;

        [Header("Flow")]
        public GameSceneSO currentScene;
        public GameSceneSO nextScene;
        public GameSceneSO mainMenuScene;
    }
}
