using UnityEngine;

public class WaypointPatroller : Patroller {
    [SerializeField] private Transform[] waypoints;
    private int currentIndex;

    public override Vector3 GetTarget() {
        if (waypoints == null || waypoints.Length == 0 || waypoints[currentIndex] == null)
            return transform.position;
        return waypoints[currentIndex].position;
    }

    public override bool IsReady() {
        return waypoints != null && waypoints.Length > 0;
    }

    public override void Advance() {
        currentIndex = (currentIndex + 1) % waypoints.Length;
    }
}
