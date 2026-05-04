using UnityEngine;
using Game.Core.Events;

public class FieldOfView : MonoBehaviour {

    [SerializeField] private LayerMask layerMask;
    [SerializeField, Range(10f, 180f)] private float fov = 40f;
    [SerializeField, Range(1f, 20f)] private float viewDistance = 3f;
    [SerializeField, Range(10, 200)] private int rayCount = 30;

    [Header("Detection")]
    [SerializeField] private VoidEventChannelSO onPlayerSpotted;

    private Mesh mesh;
    private Vector3 origin;
    private float startingAngle;
    private bool spotted;

    private void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        origin = Vector3.zero;
        startingAngle = fov / 2f;
    }

    private void LateUpdate() {
        float angle = startingAngle;
        float angleIncrease = fov / rayCount;
        
        Vector3[] vertices = new Vector3[rayCount +1 +1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = origin;

        int vertexIndex = 1;
        int triangleIndex = 0;
        for (int i = 0; i <= rayCount; i++)
        {
            Vector3 vertex;
            RaycastHit2D raycastHit2D = Physics2D.Raycast(origin, Utils.GetVectorFromAngle(angle), viewDistance, layerMask);

            if (raycastHit2D.collider == null) {
                vertex = origin + Utils.GetVectorFromAngle(angle) * viewDistance;
            } else {
                vertex = raycastHit2D.point;
            }
            vertices[vertexIndex] = vertex;
            
            if (i > 0) {
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;

                triangleIndex += 3;
            }

            vertexIndex++;
            angle -= angleIncrease;
        }
        
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.bounds = new Bounds(origin, Vector3.one * 1000f);

        DetectPlayer();
    }

    private void DetectPlayer()
    {
        if (spotted) return;
        if (Player.Instance == null) return;
        if (!IsTargetVisible(Player.Instance.GetPosition)) return;

        spotted = true;
        onPlayerSpotted?.Raise();
    }

    public bool IsTargetVisible(Vector3 target, float distanceMultiplier = 1f) {
        Vector3 toTarget = target - origin;
        float dist = toTarget.magnitude;
        float effectiveRange = viewDistance * distanceMultiplier;
        if (dist > effectiveRange || dist < 0.0001f) return false;

        Vector3 aim = Utils.GetVectorFromAngle(startingAngle - fov / 2f);
        if (Vector3.Angle(aim, toTarget.normalized) > fov / 2f) return false;

        var hit = Physics2D.Raycast(origin, toTarget.normalized, dist, layerMask);
        return hit.collider == null;
    }

    public float GetViewDistance() => viewDistance;

    private void OnDisable()
    {
        spotted = false;
    }
    
    public void SetOrigin(Vector3 origin) {
        this.origin = origin;
    }

    public void SetAimDirection(Vector3 aimDirection) {
        startingAngle = Utils.GetAngleFromVectorFloat(aimDirection) + fov / 2f;
    }

    public Vector3 GetAimDir() {
        return Utils.GetVectorFromAngle(startingAngle - fov / 2f);
    }

    public void SetFoV(float fov) {
        this.fov = fov;
    }

    public void SetViewDistance(float viewDistance) {
        this.viewDistance = viewDistance;
    }

    public bool IsPlayerVisible()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(origin, viewDistance);

        foreach (var col in colliders)
        {
            if (!col.CompareTag("Player")) continue;

            Vector3 toPlayer = col.transform.position - origin;
            Vector3 aim = Utils.GetVectorFromAngle(startingAngle - fov / 2f);

            if (Vector3.Angle(aim, toPlayer.normalized) > fov / 2f)
            {
                Debug.Log("[FOV] Player 不在角度内");
                return false;
            }

            var hit = Physics2D.Raycast(origin, toPlayer.normalized, toPlayer.magnitude, layerMask);
            if (hit.collider != null && !hit.collider.CompareTag("Player"))
            {
                Debug.Log($"[FOV] 被遮挡: {hit.collider.name}");
                return false;
            }

            Debug.Log("[FOV]  看见了");
            return true;
        }

        return false;
    }

    public bool IsSpotted => spotted; 

    public void ResetSpotted() 
    {
        spotted = false;
    }
}
