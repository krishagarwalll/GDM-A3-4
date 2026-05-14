using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MissionManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pauseMenu;
    public GameObject loseScreen;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            TogglePause();
        }
    }

    public void GameOver()
    {
        if (loseScreen != null)
        {
            loseScreen.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void TogglePause()
    {
        if (pauseMenu == null) return;
        bool isPaused = !pauseMenu.activeSelf;
        pauseMenu.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}