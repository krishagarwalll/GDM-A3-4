using UnityEngine;
using Game.Core.Events;
using Game.Core.Game;

namespace Game.UI
{
    [RequireComponent(typeof(UnityEngine.UI.Button))]
    public class RestartLevelRequester : MonoBehaviour
    {
        [SerializeField] private GameSceneEventChannelSO channel;

        private void Awake()
        {
            var levelData = LevelStarter.CurrentLevelData;
            if (levelData == null || levelData.currentScene == null)
                gameObject.SetActive(false);
        }

        public void Request()
        {
            var levelData = LevelStarter.CurrentLevelData;
            if (channel == null || levelData == null || levelData.currentScene == null)
            {
                Debug.LogError($"[RestartLevelRequester] '{name}' missing channel or levelData.currentScene.");
                return;
            }
            channel.Raise(levelData.currentScene);
        }
    }
}
