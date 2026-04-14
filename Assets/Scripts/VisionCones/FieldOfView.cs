using UnityEngine;

public class FieldOfView : MonoBehaviour {
    
    [SerializeField] private LayerMask layerMask;
    private Mesh mesh;
    private float fov;
    private float viewDistance;
    private Vector3 origin;
    private float startingAngle;

    private void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        fov = 40f;
        viewDistance = 3f;
        origin = Vector3.zero;
        startingAngle = fov / 2f;
    }

    private void LateUpdate() {
        int rayCount = 30;
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
    
}
