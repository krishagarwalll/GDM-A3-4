using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour {
    
    [SerializeField] private LayerMask layerMask;
    private Mesh mesh;

    private void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    private void Update() {
        float fov = 40f;
        Vector3 origin = Vector3.zero;
        int rayCount = 30;
        float angle = 0f;
        float angleIncrease = fov / rayCount;
        float viewDistance = 3f;
        
        Vector3[] vertices = new Vector3[rayCount +1 +1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = origin;

        int vertexIndex = 1;
        int triangleIndex = 0;
        for (int i = 0; i <= rayCount; i++)
        {
            Vector3 vertex;
            Vector3 direction = getVectorFromAngle(angle);
            Vector3 worldDirection = transform.TransformDirection(direction);
            RaycastHit2D raycastHit2D = layerMask.value == 0
                ? Physics2D.Raycast(transform.position, worldDirection, viewDistance)
                : Physics2D.Raycast(transform.position, worldDirection, viewDistance, layerMask);
            
            if (raycastHit2D.collider == null) {
                vertex = origin + direction * viewDistance;
            } else {
                vertex = transform.InverseTransformPoint(raycastHit2D.point);
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

    }
    
    public static Vector3 getVectorFromAngle(float angle) {
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }
}
