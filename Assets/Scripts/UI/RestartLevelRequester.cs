using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Core.Events;
using Game.Core.Game;

namespace Game.UI
{
    [RequireComponent(typeof(UnityEngine.UI.Button))]
    public class RestartLevelRequester : MonoBehaviour
    {
        [SerializeField] private GameSceneEventChannelSO channel;

        public void Request()
        {
            var levelData = LevelStarter.CurrentLevelData;
            if (channel != null && levelData != null && levelData.currentScene != null)
            {
                channel.Raise(levelData.currentScene);
                return;
            }

            // 回退：直接重载当前场景
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
