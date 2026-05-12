using UnityEngine;

public class Trap : MonoBehaviour
{
    [Header("Setup")]
    public MissionManager missionManager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (missionManager != null)
            {
                missionManager.GameOver();
            }
        }
    }
}