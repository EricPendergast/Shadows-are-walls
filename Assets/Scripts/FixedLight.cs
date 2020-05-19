using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Idea: Light finds all opaque objects in
// the scene, calculates all their shadows,
// and then does rendering and collisions
// stuff
[ExecuteInEditMode]
public class FixedLight : LightBase {
    public float angle;
    public float distance;

    [SerializeField]
    private MeshFilter myMeshFilter;
    private Mesh myMesh;
    [SerializeField]
    private MeshFilter castLightMeshFilter;
    private Mesh castLightMesh;

    public List<Vector2> ViewTriangle() {
        Vector3 v1 = Vector3.zero;
        Vector3 v2 = Quaternion.Euler(0f,0f, angle/2) * gameObject.transform.up;
        Vector3 v3 = Quaternion.Euler(0f,0f, -angle/2) * gameObject.transform.up;
        v2 *= distance;
        v3 *= distance;
        return new List<Vector2>{v1 + transform.position,
                                 v2 + transform.position,
                                 v3 + transform.position};
    }

    void OnDrawGizmosSelected() {
        var viewTriangle = ViewTriangle();
        for (int i = 0; i < 3; i++) {
            Gizmos.DrawLine(viewTriangle[i], viewTriangle[(i+1)%3]);
        }
    }

    void Start() {
        myMeshFilter.sharedMesh = new Mesh();
        myMesh = myMeshFilter.sharedMesh;

        castLightMeshFilter.sharedMesh = new Mesh();
        castLightMesh = castLightMeshFilter.sharedMesh;
    }

    void DrawLampshade() {
        Vector3 v1 = Vector3.zero;
        Vector3 v2 = Quaternion.Euler(0f,0f, angle/2) * Vector2.up;
        Vector3 v3 = Quaternion.Euler(0f,0f, -angle/2) * Vector2.up;

        myMesh.vertices = new []{v1, v2, v3};
        myMesh.triangles = new []{0,1,2};
    }

    void DrawShadows() {
        List<Vector3> verts = new List<Vector3>();
        var tris = new List<int>();

        var viewTriangle = ViewTriangle();

        foreach (Opaque obj in Opaque.GetAllInstances()) {
            foreach (LineSegment seg in obj.CrossSection(transform.position)) {
                var inView = seg.TriangleIntersect(viewTriangle[0], viewTriangle[1], viewTriangle[2]);
                if (inView.isValid()) {
                    Polygon.DrawQuad(
                            inView.p1,
                            Math.Extend(transform.position, inView.p1, distance),
                            Math.Extend(transform.position, inView.p2, distance),
                            inView.p2,
                            verts, tris);
                }
            }
        }

        for (int i = 0; i < verts.Count; i++) {
            verts[i] = castLightMeshFilter.transform.InverseTransformPoint(verts[i]);
        }

        castLightMesh.vertices = verts.ToArray();
        castLightMesh.triangles = tris.ToArray();
    }

    void Update() {
        DrawLampshade();
        DrawShadows();
    }
}
