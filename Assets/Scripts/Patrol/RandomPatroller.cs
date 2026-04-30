using UnityEngine;

public class RandomPatroller : Patroller {
    [SerializeField, Range(1f, 20f)] private float minDistance = 3f;
    [SerializeField, Range(1f, 20f)] private float maxDistance = 8f;
    private Vector3 currentTarget;
    private bool initialized;

    private void Start() {
        PickNewTarget();
        initialized = true;
    }

    public override Vector3 GetTarget() {
        return currentTarget;
    }

    public override bool IsReady() {
        return initialized;
    }

    public override void Advance() {
        PickNewTarget();
    }

    private void PickNewTarget() {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float distance = Random.Range(minDistance, maxDistance);
        Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
        currentTarget = transform.position + direction * distance;
    }
}
