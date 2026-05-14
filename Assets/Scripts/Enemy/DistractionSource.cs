using UnityEngine;

[DisallowMultipleComponent]
public class DistractionSource : MonoBehaviour
{
    [Header("Distraction")]
    [SerializeField] private DistractionIntensity intensity = DistractionIntensity.Loud;
    [Tooltip("Guards within this radius receive the distraction.")]
    [SerializeField, Range(1f, 30f)] private float audibleRadius = 8f;

    [Header("Auto-emit")]
    [Tooltip("Emit once when this component becomes enabled.")]
    [SerializeField] private bool emitOnEnable = false;

    private void OnEnable()
    {
        if (emitOnEnable) Emit();
    }

    public void Emit()
    {
        Vector3 pos = transform.position;
        float r2 = audibleRadius * audibleRadius;
        var guards = Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        for (int i = 0; i < guards.Length; i++)
        {
            var g = guards[i];
            if (g == null || g.IsKnockedOut) continue;
            if ((g.transform.position - pos).sqrMagnitude > r2) continue;
            g.OnDistraction(pos, intensity);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = intensity == DistractionIntensity.Loud
            ? new Color(1f, 0.4f, 0.2f, 0.35f)
            : new Color(1f, 0.85f, 0.3f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, audibleRadius);
    }
}
