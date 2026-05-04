using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(FieldOfView))]
public class VisionSweep : MonoBehaviour {
    [SerializeField, Range(10f, 270f)] private float sweepAngle = 90f;
    [SerializeField, Range(0.5f, 15f)] private float sweepPeriod = 4f;
    [SerializeField] private Vector2 baseDirection = Vector2.right;

    private FieldOfView fov;
    private float t;

    private void Awake() {
        fov = GetComponent<FieldOfView>();
    }

    private void Update() {
        if (fov == null) return;

        t += Time.deltaTime;
        float phase = (t / Mathf.Max(0.0001f, sweepPeriod)) * Mathf.PI * 2f;
        float offset = Mathf.Sin(phase) * (sweepAngle * 0.5f);

        Vector3 baseDir = baseDirection.sqrMagnitude > 0.0001f
            ? (Vector3)baseDirection.normalized
            : Vector3.right;

        float baseAngle = Utils.GetAngleFromVectorFloat(baseDir);
        Vector3 dir = Utils.GetVectorFromAngle(baseAngle + offset);
        fov.SetAimDirection(dir);
    }

    public void SetBaseDirection(Vector3 worldDir) {
        if (worldDir.sqrMagnitude > 0.0001f) {
            baseDirection = ((Vector2)worldDir).normalized;
        }
    }

    public Vector3 GetCurrentSweepDirection() {
        Vector3 baseDir = baseDirection.sqrMagnitude > 0.0001f
            ? (Vector3)baseDirection.normalized
            : Vector3.right;
        float baseAngle = Utils.GetAngleFromVectorFloat(baseDir);
        float phase = (t / Mathf.Max(0.0001f, sweepPeriod)) * Mathf.PI * 2f;
        float offset = Mathf.Sin(phase) * (sweepAngle * 0.5f);
        return Utils.GetVectorFromAngle(baseAngle + offset);
    }
}
