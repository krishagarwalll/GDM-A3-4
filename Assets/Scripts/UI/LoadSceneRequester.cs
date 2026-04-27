using UnityEngine;
using Game.Core.Events;
using Game.Core.Scenes;

namespace Game.UI
{
    /// <summary>
    /// Drop on a UI Button (or any GameObject). Wire its onClick to Request().
    /// Raises a GameSceneEventChannelSO so the SceneLoader (in _PersistentManagers)
    /// loads the requested scene — without holding a direct reference to the loader.
    /// </summary>
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
