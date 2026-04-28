using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Core.Events;

namespace Game.Core.Scenes
{
    public class Bootstrapper : MonoBehaviour
    {
        [Header("Persistent scene")]
        [SerializeField] private string persistentManagersSceneName = "_PersistentManagers";

        [Header("First scene")]
        [SerializeField] private GameSceneEventChannelSO loadSceneRequest;
        [SerializeField] private GameSceneSO firstSceneToLoad;

        [Header("Behaviour")]
        [SerializeField] private bool unloadSelfAfterBoot = true;

        private IEnumerator Start()
        {
            var existing = SceneManager.GetSceneByName(persistentManagersSceneName);
            if (!existing.IsValid() || !existing.isLoaded)
                yield return SceneManager.LoadSceneAsync(persistentManagersSceneName, LoadSceneMode.Additive);

            yield return null;

            if (loadSceneRequest != null && firstSceneToLoad != null)
                loadSceneRequest.Raise(firstSceneToLoad);
            else
                Debug.LogError("[Bootstrapper] loadSceneRequest or firstSceneToLoad not assigned.");

            if (unloadSelfAfterBoot)
            {
                yield return null;
                var self = gameObject.scene;
                if (self.IsValid())
                    yield return SceneManager.UnloadSceneAsync(self);
            }
        }
    }
}
