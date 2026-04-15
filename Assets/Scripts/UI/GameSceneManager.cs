using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    public enum SceneIndex
    {
        MainMenu = 0,
        LevelSelect = 1,
        tutorialLevel = 2
    }

    public static GameSceneManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.rKey.wasPressedThisFrame)
            ReloadCurrentScene();
    }

    // For use in code
    public void LoadScene(SceneIndex scene)
    {
        StartCoroutine(LoadRoutine((int)scene));
    }

    // For use on UI buttons in the Inspector
    public void LoadScene(int buildIndex)
    {
        StartCoroutine(LoadRoutine(buildIndex));
    }

    public void ReloadCurrentScene()
    {
        StartCoroutine(LoadRoutine(SceneManager.GetActiveScene().buildIndex));
    }

    private IEnumerator LoadRoutine(int buildIndex)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(buildIndex);

        while (!op.isDone)
            yield return null;
    }
}
