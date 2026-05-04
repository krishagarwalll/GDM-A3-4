using UnityEngine;

[DisallowMultipleComponent]
public class StaticPatroller : Patroller {
    [Tooltip("Optional override post. If empty, the guard's spawn position is used.")]
    [SerializeField] private Transform postOverride;

    private Vector3 home;
    private bool initialized;

    private void Awake() {
        InitializeHome();
    }

    private void InitializeHome() {
        if (initialized) return;
        home = postOverride != null ? postOverride.position : transform.position;
        initialized = true;
    }

    public override Vector3 GetTarget() {
        if (!initialized) InitializeHome();
        return home;
    }

    public override bool IsReady() => true;

    public override void Advance() { }
}
