using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Game.Core.Events;

namespace Game.Core.Scenes
{
    public class SceneLoader : MonoBehaviour
    {
        [Header("Channels (input)")]
        [SerializeField] private GameSceneEventChannelSO loadSceneRequest;

        [Header("Channels (output)")]
        [SerializeField] private VoidEventChannelSO onSceneLoaded;

        [Header("Hotkeys")]
        [SerializeField] private bool reloadOnRKey = true;

        [Header("Transition")]
        [Tooltip("Optional. If null, falls back to SceneTransition.Instance.")]
        [SerializeField] private SceneTransition transition;

        private GameSceneSO _currentScene;
        private bool _isLoading;

        private SceneTransition Transition => transition != null ? transition : SceneTransition.Instance;

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

        public void LoadScene(GameSceneSO scene) => HandleLoadRequest(scene);

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

            var t = Transition;
            if (t != null)
                yield return t.RunBetween(SwapScenes(target));
            else
                yield return SwapScenes(target);

            _currentScene = target;
            _isLoading = false;

            if (onSceneLoaded != null)
                onSceneLoaded.Raise();
        }

        private IEnumerator SwapScenes(GameSceneSO target)
        {
            if (_currentScene != null)
            {
                var loaded = SceneManager.GetSceneByName(_currentScene.sceneName);
                if (loaded.IsValid() && loaded.isLoaded)
                    yield return SceneManager.UnloadSceneAsync(loaded);
            }

            var op = SceneManager.LoadSceneAsync(target.sceneName, LoadSceneMode.Additive);
            yield return op;

            var newScene = SceneManager.GetSceneByName(target.sceneName);
            if (newScene.IsValid())
                SceneManager.SetActiveScene(newScene);
        }
    }
}
