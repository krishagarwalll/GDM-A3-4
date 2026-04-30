using UnityEngine;

public abstract class Patroller : MonoBehaviour {
    [SerializeField, Range(0.05f, 2f)] protected float reachThreshold = 0.2f;

    public abstract Vector3 GetTarget();
    public abstract bool IsReady();
    public abstract void Advance();

    public bool HasReached(Vector3 currentPosition) {
        return IsReady() && Vector3.Distance(currentPosition, GetTarget()) <= reachThreshold;
    }
}
