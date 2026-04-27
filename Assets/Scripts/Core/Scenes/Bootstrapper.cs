using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Core.Events;

namespace Game.Core.Scenes
{
    /// <summary>
    /// Lives in the _Bootstrap scene (build index 0). Its single job:
    ///   1. Additively load _PersistentManagers (which contains SceneLoader, audio, etc.)
    ///   2. Raise the load-scene channel to request the first real scene (e.g. mainMenu)
    ///   3. Optionally unload itself once everything else is up
    ///
    /// Run the game from any scene during development — if _PersistentManagers isn't
    /// loaded yet, you'll just be missing managers; load this scene first to get the
    /// full pipeline.
    /// </summary>
    public class Bootstrapper : MonoBehaviour
    {
        [Header("Persistent scene")]
        [Tooltip("Name of the persistent managers scene to additively load.")]
        [SerializeField] private string persistentManagersSceneName = "_PersistentManagers";

        [Header("First scene")]
        [SerializeField] private GameSceneEventChannelSO loadSceneRequest;
        [SerializeField] private GameSceneSO firstSceneToLoad;

        [Header("Behaviour")]
        [Tooltip("Unload this Bootstrap scene after the first scene is loaded.")]
        [SerializeField] private bool unloadSelfAfterBoot = true;

        private IEnumerator Start()
        {
            // 1. Load PersistentManagers if it isn't already
            var existing = SceneManager.GetSceneByName(persistentManagersSceneName);
            if (!existing.IsValid() || !existing.isLoaded)
                yield return SceneManager.LoadSceneAsync(persistentManagersSceneName, LoadSceneMode.Additive);

            // Give listeners (SceneLoader.OnEnable) one frame to subscribe
            yield return null;

            // 2. Request the first real scene via the channel
            if (loadSceneRequest != null && firstSceneToLoad != null)
                loadSceneRequest.Raise(firstSceneToLoad);
            else
                Debug.LogError("[Bootstrapper] loadSceneRequest or firstSceneToLoad not assigned.");

            // 3. Optionally drop the bootstrap scene
            if (unloadSelfAfterBoot)
            {
                // Wait one frame so the load request actually starts
                yield return null;
                var self = gameObject.scene;
                if (self.IsValid())
                    yield return SceneManager.UnloadSceneAsync(self);
            }
        }
    }
}
