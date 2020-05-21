using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Idea: Light finds all opaque objects in
// the scene, calculates all their shadows,
// and then does rendering and collisions
// stuff
[RequireComponent (typeof (PolygonCollider2D))]
public class FixedLight : LightBase {
    public float angle;
    public float distance;

    [SerializeField]
    private MeshFilter myMeshFilter;
    private Mesh myMesh;
    [SerializeField]
    private MeshFilter castLightMeshFilter;
    private Mesh castLightMesh;

    [SerializeField]
    private PolygonCollider2D visibleCollider;

    [SerializeField]
    private List<Quad> shadows;

    public Triangle LocalViewTriangle() {
        Vector3 v1 = Vector3.zero;
        Vector3 v2 = Quaternion.Euler(0f,0f, angle/2) * Vector2.up;
        Vector3 v3 = Quaternion.Euler(0f,0f, -angle/2) * Vector2.up;
        v2 *= distance;
        v3 *= distance;

        return new Triangle(v1, v2, v3);
    }

    public Triangle ViewTriangle() {
        Triangle loc = LocalViewTriangle();
        loc.p1 = transform.TransformPoint(loc.p1);
        loc.p2 = transform.TransformPoint(loc.p2);
        loc.p3 = transform.TransformPoint(loc.p3);
        return loc;
    }

    void OnDrawGizmos() {
        var viewTriangle = ViewTriangle();
        foreach (var side in ViewTriangle().GetSides()) {
            Gizmos.DrawLine(side.p1, side.p2);
        }
    }

    void Start() {
        myMeshFilter.sharedMesh = new Mesh();
        myMesh = myMeshFilter.sharedMesh;

        castLightMeshFilter.sharedMesh = new Mesh();
        castLightMesh = castLightMeshFilter.sharedMesh;

        visibleCollider = GetComponent<PolygonCollider2D>();

        visibleCollider.SetPath(0, ViewTriangle().AsList());
    }

    void DrawLampshade() {
        Vector3 v1 = Vector3.zero;
        Vector3 v2 = Quaternion.Euler(0f,0f, angle/2) * Vector2.up;
        Vector3 v3 = Quaternion.Euler(0f,0f, -angle/2) * Vector2.up;

        myMesh.vertices = new []{v1, v2, v3};
        myMesh.triangles = new []{0,1,2};
    }


    // Returns the shape of the shadow of seg, where the first point in the
    // returned quad is analogous to seg.p1, and the last point is analogous to
    // seg.p2. This ordering is important to ShadowEdge.GetTarget()
    public override Quad? GetShadowShape(LineSegment seg) {
        var inView = seg.Intersect(ViewTriangle());
        if (inView.isValid()) {
            return new Quad(
                    inView.p1,
                    Math.Extend(transform.position, inView.p1, distance),
                    Math.Extend(transform.position, inView.p2, distance),
                    inView.p2);
        }
        return null;
    }

    void DrawShadows() {
        if (shadows == null) {
            shadows = new List<Quad>();
        }
        shadows.Clear();
        List<Vector3> verts = new List<Vector3>();
        var tris = new List<int>();
        
        var viewTriangle = ViewTriangle();
        
        foreach (Opaque obj in Opaque.GetAllInstances()) {
            Quad? shadowShape = GetShadowShape(obj.CrossSection(transform.position));
            if (shadowShape is Quad s) {
                shadows.Add(s);
            }
        }
        
        //foreach (var quad in shadows) {
        //    quad.Draw(verts, tris);
        //}
        
        //for (int i = 0; i < verts.Count; i++) {
        //    verts[i] = castLightMeshFilter.transform.InverseTransformPoint(verts[i]);
        //}
        
        // TODO: This may not be optimal
        // TODO: Is this really the way this error is supposed to be fixed?
        //if (castLightMesh.vertexCount < verts.Count) {
        //    castLightMesh.vertices = verts.ToArray();
        //    castLightMesh.triangles = tris.ToArray();
        //} else {
        //    castLightMesh.triangles = tris.ToArray();
        //    castLightMesh.vertices = verts.ToArray();
        //}
    }

    void Update() {
        visibleCollider.SetPath(0, LocalViewTriangle().AsList());
        DrawLampshade();
        DrawShadows();

        if (IsInDark(Mouse.WorldPosition())) {
            Debug.Log("Mouse in dark");
        } else {
            Debug.Log("Mouse in light");
        }
    }

    void FixedUpdate() {
        // talk to Shadows.instance
        // get the slice points for each shadow boundary
        // update colliders accordingly
    }

    public override bool IsInDark(Vector2 point) {
        if (!this.visibleCollider.OverlapPoint(point)) {
            return true;
        }
        // TODO: Check if the point is outside of the view triangle
        foreach (var quad in shadows) {
            if (quad.Contains(point)) {
                return true;
            }
        }

        return false;
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.TryGetComponent(out Opaque opaque)) {
            opaque.CreateShadow(this);
        }
    }

    void OnCollisionExit2D(Collision2D collision) {
        if (collision.gameObject.TryGetComponent(out Opaque opaque)) {
            opaque.DestroyShadow(this);
        }
    }
}
