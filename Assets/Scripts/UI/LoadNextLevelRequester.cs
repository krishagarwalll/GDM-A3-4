using UnityEngine;
using Game.Core.Events;
using Game.Core.Game;

namespace Game.UI
{
    [RequireComponent(typeof(UnityEngine.UI.Button))]
    public class LoadNextLevelRequester : MonoBehaviour
    {
        [SerializeField] private GameSceneEventChannelSO channel;
        [SerializeField] private LevelDataSO levelData;

        private void Awake()
        {
            if (levelData == null || levelData.nextScene == null)
                gameObject.SetActive(false);
        }

        public void Request()
        {
            if (channel == null || levelData == null || levelData.nextScene == null)
            {
                Debug.LogError($"[LoadNextLevelRequester] '{name}' missing channel or levelData.nextScene.");
                return;
            }
            channel.Raise(levelData.nextScene);
        }
    }
}
