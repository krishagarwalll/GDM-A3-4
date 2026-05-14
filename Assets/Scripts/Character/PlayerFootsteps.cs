using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Core.Pause;

[RequireComponent(typeof(Player))]
public class PlayerFootsteps : MonoBehaviour
{
    private static readonly string[] FootstepScenes = { "level1Scene", "level2Scene", "level3" };

    [Header("Emission")]
    [Tooltip("Seconds between footstep distractions while moving uncrouched.")]
    [SerializeField, Range(0.1f, 2f)] private float stepInterval = 0.4f;
    [Tooltip("Guards within this radius receive the footstep distraction.")]
    [SerializeField, Range(1f, 20f)] private float audibleRadius = 6f;

    private Player player;
    private float timer;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void Update()
    {
        if (PauseService.IsPaused) return;
        if (!IsFootstepScene()) return;

        if (player == null || !player.IsMoving || player.IsCrouching)
        {
            timer = 0f;
            return;
        }

        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = stepInterval;

        EmitFootstep();
    }

    private void EmitFootstep()
    {
        Vector3 pos = transform.position;
        float r2 = audibleRadius * audibleRadius;
        var guards = Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        for (int i = 0; i < guards.Length; i++)
        {
            var g = guards[i];
            if (g == null || g.IsKnockedOut) continue;
            if ((g.transform.position - pos).sqrMagnitude > r2) continue;
            g.OnDistraction(pos, DistractionIntensity.Footstep);
        }
    }

    private static bool IsFootstepScene()
    {
        string active = SceneManager.GetActiveScene().name;
        for (int i = 0; i < FootstepScenes.Length; i++)
        {
            if (FootstepScenes[i] == active) return true;
        }
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 0.8f, 1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, audibleRadius);
    }
}
