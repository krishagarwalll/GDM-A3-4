using UnityEngine;
using Game.Core.Events;
using Game.Core.Scenes;

namespace Game.UI
{
    public class LoadSceneRequester : MonoBehaviour
    {
        [SerializeField] private GameSceneEventChannelSO channel;
        [SerializeField] private GameSceneSO scene;

        public void Request()
        {
            if (channel == null || scene == null)
            {
                Debug.LogError($"[LoadSceneRequester] '{name}' missing channel or scene reference.");
                return;
            }
            channel.Raise(scene);
        }
    }
}
