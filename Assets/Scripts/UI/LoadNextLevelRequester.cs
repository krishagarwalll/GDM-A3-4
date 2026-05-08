using UnityEngine;
using Game.Core.Events;
using Game.Core.Game;

namespace Game.UI
{
    [RequireComponent(typeof(UnityEngine.UI.Button))]
    public class LoadNextLevelRequester : MonoBehaviour
    {
        [SerializeField] private GameSceneEventChannelSO channel;

        private void Awake()
        {
            var levelData = LevelStarter.CurrentLevelData;
            if (levelData == null || levelData.nextScene == null)
                gameObject.SetActive(false);
        }

        public void Request()
        {
            var levelData = LevelStarter.CurrentLevelData;
            if (channel == null || levelData == null || levelData.nextScene == null)
            {
                Debug.LogError($"[LoadNextLevelRequester] '{name}' missing channel or levelData.nextScene.");
                return;
            }
            channel.Raise(levelData.nextScene);
        }
    }
}
