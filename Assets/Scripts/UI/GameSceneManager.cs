using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.rKey.wasPressedThisFrame)
            ReloadCurrentScene();
    }

    public void ReloadCurrentScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.buildIndex);
    }
}
