using UnityEngine;
using System.Collections;

public class Level5GameController : MonoBehaviour
{
    public static Level5GameController Instance { get; private set; }

    public bool IsLightOn { get; private set; } = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
   
    private IEnumerator Start()
    {
        yield return null; 
        NotifyAllEnemies();
    }
    public void SetLightState(bool lightOn)
    {
        if (IsLightOn == lightOn) return;
        IsLightOn = lightOn;
        NotifyAllEnemies();
    }

    private void NotifyAllEnemies()
    {
        foreach (var enemy in FindObjectsByType<EnemyAI>(FindObjectsSortMode.None))
            enemy.SetDarkMode(!IsLightOn);
    }
}
