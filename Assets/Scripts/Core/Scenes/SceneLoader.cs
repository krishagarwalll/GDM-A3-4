using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Game.Core.Events;

namespace Game.Core.Scenes
{
    /// <summary>
    /// Lives in the _PersistentManagers scene. Listens on a GameSceneEventChannelSO
    /// for load-scene requests, then unloads the current gameplay scene and
    /// additively loads the requested one.
    ///
    /// Anyone who wants to change scenes raises the channel — they never call
    /// SceneManager directly. This means UI buttons hold no cross-scene references
    /// and can't break when scenes reload.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        [Header("Channels (input)")]
        [SerializeField] private GameSceneEventChannelSO loadSceneRequest;

        [Header("Channels (output)")]
        [SerializeField] private VoidEventChannelSO onSceneLoaded;

        [Header("Hotkeys")]
        [Tooltip("Press to reload the currently active gameplay scene.")]
        [SerializeField] private bool reloadOnRKey = true;

        // Tracks which gameplay scene is currently loaded on top of _PersistentManagers
        private GameSceneSO _currentScene;
        private bool _isLoading;

        private void OnEnable()
        {
            if (loadSceneRequest != null)
                loadSceneRequest.OnEventRaised += HandleLoadRequest;
        }

        private void OnDisable()
        {
            if (loadSceneRequest != null)
                loadSceneRequest.OnEventRaised -= HandleLoadRequest;
        }

        private void Update()
        {
            if (!reloadOnRKey || _currentScene == null || _isLoading) return;
            var kb = Keyboard.current;
            if (kb != null && kb.rKey.wasPressedThisFrame)
                HandleLoadRequest(_currentScene);
        }

        private void HandleLoadRequest(GameSceneSO scene)
        {
            if (scene == null)
            {
                Debug.LogWarning("[SceneLoader] Received null scene on load request.");
                return;
            }
            if (_isLoading)
            {
                Debug.LogWarning($"[SceneLoader] Ignoring request for '{scene.sceneName}' — load already in progress.");
                return;
            }
            StartCoroutine(LoadRoutine(scene));
        }

        private IEnumerator LoadRoutine(GameSceneSO target)
        {
            _isLoading = true;

            // Unload the previously loaded gameplay scene if any
            if (_currentScene != null && _currentScene != target)
            {
                var loaded = SceneManager.GetSceneByName(_currentScene.sceneName);
                if (loaded.IsValid() && loaded.isLoaded)
                    yield return SceneManager.UnloadSceneAsync(loaded);
            }
            else if (_currentScene == target)
            {
                // Reload: unload + reload
                var loaded = SceneManager.GetSceneByName(target.sceneName);
                if (loaded.IsValid() && loaded.isLoaded)
                    yield return SceneManager.UnloadSceneAsync(loaded);
            }

            // Additively load the target
            var op = SceneManager.LoadSceneAsync(target.sceneName, LoadSceneMode.Additive);
            yield return op;

            // Make it the active scene so newly-instantiated objects land in it
            var newScene = SceneManager.GetSceneByName(target.sceneName);
            if (newScene.IsValid())
                SceneManager.SetActiveScene(newScene);

            _currentScene = target;
            _isLoading = false;

            if (onSceneLoaded != null)
                onSceneLoaded.Raise();
        }
    }
}
