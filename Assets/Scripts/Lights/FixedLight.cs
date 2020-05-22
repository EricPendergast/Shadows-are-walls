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

    //void GetShadows() {
    //    if (shadows == null) {
    //        shadows = new List<Quad>();
    //    }
    //    shadows.Clear();
    //    
    //    var viewTriangle = ViewTriangle();
    //    
    //    foreach (Opaque obj in Opaque.GetAllInstances()) {
    //        Quad? shadowShape = GetShadowShape(obj.CrossSection(transform.position));
    //        if (shadowShape is Quad s) {
    //            shadows.Add(s);
    //        }
    //    }
    //}

    // TODO: DrawShadows is no longer what we want
    //void DrawShadows() {
    //    GetShadows();
    //
    //    List<Vector3> verts = new List<Vector3>();
    //    var tris = new List<int>();
    //    
    //    foreach (var quad in shadows) {
    //        quad.Draw(verts, tris);
    //    }
    //    
    //    for (int i = 0; i < verts.Count; i++) {
    //        verts[i] = transform.InverseTransformPoint(verts[i]);
    //    }
    //    
    //    // TODO: This may not be optimal
    //    // TODO: Is this really the way this error is supposed to be fixed?
    //    if (shadowMesh.vertexCount < verts.Count) {
    //        shadowMesh.vertices = verts.ToArray();
    //        shadowMesh.triangles = tris.ToArray();
    //    } else {
    //        shadowMesh.triangles = tris.ToArray();
    //        shadowMesh.vertices = verts.ToArray();
    //    }
    //}

    // TODO: This should draw the actual casted light (the whole view triangle
    // with shadows subtracted)
    void DrawCastedLight() {
        var t = LocalViewTriangle();
        this.castLightMesh.vertices = new Vector3[]{t.p1, t.p2, t.p3};
        //this.castLightMesh.colors = new Color[]{0, 0, 0};
        this.castLightMesh.triangles = new int[]{0, 1, 2};
    }

    List<LineSegment> MinimalUnion(IEnumerable<LineSegment> shadowsIn, Vector2 convergencePoint) {
        var rightEdge = RightEdge();
        System.Func<Vector2, float> metric = (Vector2 p) => {
            return rightEdge.Angle(p);
        };

        var allShadows = new List<Cup>();

        System.Action<LineSegment> addShadow = (LineSegment s) => {
            if (metric(s.p1) > metric(s.p2)) {
                s.Swap();
            }
            allShadows.Add(new Cup(s, convergencePoint));
        };

        foreach (var shadow in shadows.Values) {
            addShadow(shadow.caster.CrossSection(convergencePoint));
        }
        addShadow(FarEdge());

        allShadows.Sort((s1, s2) => metric(s1.p1).CompareTo(metric(s2.p1)));

        var minimalUnion = new List<LineSegment>();

        var toTrim = new Queue<Cup>();
        toTrim.Enqueue(allShadows[0]);
        
        for (int i = 1; i < allShadows.Count; i++) {
            var nextShadow = allShadows[i];
            int toTrimCount = toTrim.Count;
            for (int j = 0; j < toTrimCount; j++) {
                Cup c = toTrim.Dequeue();
                if (metric(c.p2) < metric(nextShadow.p1)) {
                    minimalUnion.Add(c.Base());
                } else {
                    toTrim.Enqueue(c);
                }
            }
            Math.MinimalUnion(toTrim, nextShadow);
        }

        foreach (Cup c in toTrim) {
            minimalUnion.Add(c.Base());
        }

        return minimalUnion;
    }

    void Update() {
        visibleCollider.SetPath(0, LocalViewTriangle().AsList());

        //DrawShadows();
        DrawCastedLight();

        var allShadows = new List<LineSegment>();
        foreach (Shadow s in shadows.Values) {
            allShadows.Add(s.caster.CrossSection(transform.position));
        }
        trimmedShadows = MinimalUnion(allShadows, transform.position);

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

    // TODO: This can use the new generated light mesh
    public override bool IsInDark(Vector2 point) {
        if (!this.visibleCollider.OverlapPoint(point)) {
            return true;
        }
        // TODO: this is now broken (and is also really important)
        //foreach (var quad in shadows) {
        //    if (quad.Contains(point)) {
        //        return true;
        //    }
        //}

        return false;
    }

    // TODO: create the shadow, and keep track of which opaque object it is from.
    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.TryGetComponent(out Opaque opaque)) {
            Shadow s = Util.CreateChild<Shadow>(transform);
            s.Init(opaque);
            shadows.Add(opaque.GetInstanceID(), s);
        }
    }

    // TODO: do the right things so that above still works
    void OnCollisionExit2D(Collision2D collision) {
        if (collision.gameObject.TryGetComponent(out Opaque opaque)) {
            Shadow s = shadows[opaque.GetInstanceID()];
            Destroy(s);
            shadows.Remove(opaque.GetInstanceID());
        }
    }
}
