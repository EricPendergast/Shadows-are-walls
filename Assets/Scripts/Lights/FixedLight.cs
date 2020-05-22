using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Idea: Light finds all opaque objects in
// the scene, calculates all their shadows,
// and then does rendering and collisions
// stuff
public class FixedLight : LightBase {
    public float angle;
    public float distance;

    private Mesh castLightMesh;
    // TODO: Don't do anything with this anymore
    private Mesh shadowMesh;

    private PolygonCollider2D visibleCollider;

    //[SerializeField]
    //private List<Quad> shadows;
    
    private Dictionary<int, Shadow> shadows = new Dictionary<int, Shadow>();

    // All visible shadows. This does not contain Shadow objects because
    // sometimes shadows get split into multiple line segments
    private List<LineSegment> trimmedShadows = new List<LineSegment>();

    public LineSegment FarEdge() {
        return new LineSegment(ViewTriangle().p2, ViewTriangle().p3);
    }

    public LineSegment RightEdge() {
        return new LineSegment(ViewTriangle().p1, ViewTriangle().p3);
    }

    public LineSegment LeftEdge() {
        return new LineSegment(ViewTriangle().p1, ViewTriangle().p2);
    }

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
        OnDrawGizmosSelected();
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        foreach (var seg in trimmedShadows) {
            Gizmos.DrawLine(seg.p1, seg.p2);
        }
        Gizmos.color = Color.white;
    }

    public override void Awake() {
        base.Awake();
        for (int i = 0; i < 3; i++) {
            int capturedI = i;
            Util.CreateChild<CustomShadowEdge>(transform).Init(() => {
                return ViewTriangle().GetSides()[capturedI];
            });
        }

        Refs.instance.lightMaterial.SetInt("lightId", lightCounter);
        Refs.instance.shadowMaterial.SetInt("lightId", lightCounter);
        lightCounter++;

        castLightMesh = Util.CreateMeshWithNewMaterial(gameObject, Refs.instance.lightMaterial);

        GameObject c = Util.CreateChild(transform);
        c.transform.localPosition = Vector3.zero;
        c.transform.localRotation = Quaternion.identity;
        shadowMesh = Util.CreateMeshWithNewMaterial(c, Refs.instance.shadowMaterial);

        visibleCollider = gameObject.AddComponent<PolygonCollider2D>();
        visibleCollider.SetPath(0, ViewTriangle().AsList());
    }

    //void DrawLampshade() {
    //    Vector3 v1 = Vector3.zero;
    //    Vector3 v2 = Quaternion.Euler(0f,0f, angle/2) * Vector2.up;
    //    Vector3 v3 = Quaternion.Euler(0f,0f, -angle/2) * Vector2.up;
    //
    //    myMesh.vertices = new []{v1, v2, v3};
    //    myMesh.triangles = new []{0,1,2};
    //}


    // Returns the shape of the shadow of seg, where the first point in the
    // returned quad is analogous to seg.p1, and the last point is analogous to
    // seg.p2. This ordering is important to ShadowEdge.GetTarget()
    public override Quad? GetShadowShape(LineSegment seg) {
        return null;
        //var inView = seg.Intersect(ViewTriangle());
        //if (inView.isValid()) {
        //    return new Quad(
        //            inView.p1,
        //            Math.Extend(transform.position, inView.p1, distance),
        //            Math.Extend(transform.position, inView.p2, distance),
        //            inView.p2);
        //}
        //return null;
    }

    void DrawCastedLight() {

        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        vertices.Add(transform.position);

        foreach (LineSegment shadow in trimmedShadows) {
            triangles.Add(vertices.Count);
            vertices.Add(shadow.p1);
            triangles.Add(vertices.Count);
            vertices.Add(shadow.p2);
            triangles.Add(0);
        }

        for (int i = 0; i < vertices.Count; i++) {
            vertices[i] = transform.InverseTransformPoint(vertices[i]);
        }

        if (castLightMesh.vertexCount < vertices.Count) {
            castLightMesh.vertices = vertices.ToArray();
            castLightMesh.triangles = triangles.ToArray();
        } else {
            castLightMesh.triangles = triangles.ToArray();
            castLightMesh.vertices = vertices.ToArray();
        }
    }

    void Update() {
        visibleCollider.SetPath(0, LocalViewTriangle().AsList());

        DrawCastedLight();

        var allShadows = new List<LineSegment>();
        foreach (Shadow s in shadows.Values) {
            allShadows.Add(s.caster.CrossSection(transform.position));
        }
        allShadows.Add(FarEdge());
        trimmedShadows = Math.MinimalUnion(allShadows, transform.position, RightEdge());

        //if (IsInDark(Mouse.WorldPosition())) {
        //    Debug.Log("Mouse in dark");
        //} else {
        //    Debug.Log("Mouse in light");
        //}
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

        foreach (LineSegment seg in trimmedShadows) {
            if (Math.IsInTriangle(point, seg.p1, seg.p2, transform.position)) {
                return false;
            }
        }
        return true;
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.TryGetComponent(out Opaque opaque)) {
            Shadow s = Util.CreateChild<Shadow>(transform);
            s.Init(opaque);
            shadows.Add(opaque.GetInstanceID(), s);
        }
    }

    void OnCollisionExit2D(Collision2D collision) {
        if (collision.gameObject.TryGetComponent(out Opaque opaque)) {
            Shadow s = shadows[opaque.GetInstanceID()];
            Destroy(s);
            shadows.Remove(opaque.GetInstanceID());
        }
    }
}
