using UnityEngine;

public class WaypointPatroller : Patroller {
    [SerializeField] private Transform[] waypoints;
    private int currentIndex;

    public override Vector3 GetTarget() {
        return waypoints[currentIndex].position;
    }

    public override bool IsReady() {
        return waypoints != null && waypoints.Length > 0;
    }

    public override void Advance() {
        currentIndex = (currentIndex + 1) % waypoints.Length;
    }
}
