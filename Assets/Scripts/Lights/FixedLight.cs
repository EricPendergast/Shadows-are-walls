using System.Collections.Generic;
using UnityEngine;

// Idea: Light finds all opaque objects in
// the scene, calculates all their shadows,
// and then does rendering and collisions
// stuff
[RequireComponent(typeof(Rigidbody2D))]
public class FixedLight : LightBase {
    public float targetAngle;
    public float distance;

    private Mesh castLightMesh;

    private PolygonCollider2D visibleCollider;

    private Dictionary<Opaque, Shadow> shadows = new Dictionary<Opaque, Shadow>();

    private LightEdge rightShadowEdge;
    private LightEdge leftShadowEdge;
    private LightEdge farShadowEdge;

    // All visible shadows. This does not contain Shadow objects because
    // sometimes shadows get split into multiple line segments
    private List<LineSegment> trimmedShadows = new List<LineSegment>();

    private Triangle actualViewTriangle;
    private Triangle targetViewTriangle;

    [SerializeField]
    private GameObject shadowParent;

    public void SetTargetApertureAngle(float v) {
        targetAngle = v;
    }

    public float GetActualApertureAngle() {
        return ActualLeftEdge().UnsignedAngle(ActualRightEdge());
    }

    public float GetActualAngle() {
        var left = ActualLeftEdge();
        var right = ActualRightEdge();
        return new LineSegment((left.p1 + right.p1)/2, (left.p2 + right.p2)/2).Angle();
    }

    public void SetTargetAngle(float v) {
        transform.localRotation = Quaternion.Euler(0, 0, v);
    }

    public void SetTargetPosition(Vector2 p) {
        transform.position = p;
    }

    public override Vector2 GetTargetPosition() {
        return transform.position;
    }

    public Vector2 GetActualPosition() {
        return (ActualLeftEdge().p1 + ActualRightEdge().p1)/2;
    }

    public LineSegment ActualFarEdge() {
        return new LineSegment(actualViewTriangle.p2, actualViewTriangle.p3);
    }

    public LineSegment ActualRightEdge() {
        return new LineSegment(actualViewTriangle.p1, actualViewTriangle.p3);
    }

    public LineSegment ActualLeftEdge() {
        return new LineSegment(actualViewTriangle.p1, actualViewTriangle.p2);
    }

    public LineSegment TargetFarEdge() {
        return new LineSegment(targetViewTriangle.p2, targetViewTriangle.p3);
    }

    public LineSegment TargetRightEdge() {
        return new LineSegment(targetViewTriangle.p1, targetViewTriangle.p3);
    }

    public LineSegment TargetLeftEdge() {
        return new LineSegment(targetViewTriangle.p1, targetViewTriangle.p2);
    }

    void CacheViewTriangles() {
        targetViewTriangle = CalculateTargetViewTriangle();
        actualViewTriangle = CalculateActualViewTriangle();
    }

    public Triangle CalculateTargetLocalViewTriangle() {
        Vector3 v1 = Vector3.zero;
        Vector3 v2 = Quaternion.Euler(0f,0f, targetAngle/2) * Vector2.up;
        Vector3 v3 = Quaternion.Euler(0f,0f, -targetAngle/2) * Vector2.up;
        v2 *= distance;
        v3 *= distance;
        
        return new Triangle(v1, v2, v3);
    }

    public Triangle CalculateTargetViewTriangle() {
        Triangle loc = CalculateTargetLocalViewTriangle();
        return new Triangle(
                transform.TransformPoint(loc.p1),
                transform.TransformPoint(loc.p2),
                transform.TransformPoint(loc.p3));
    }

    public Triangle CalculateActualViewTriangle() {
        var leftSeg = leftShadowEdge.GetDivider().WithLength(distance);
        var rightSeg = rightShadowEdge.GetDivider().WithLength(distance);
        if (leftSeg.p1 == leftSeg.p2 || rightSeg.p1 == rightSeg.p2) {
            return CalculateTargetViewTriangle();
        }
        return new Triangle(leftSeg.p1, leftSeg.p2, rightSeg.p2);
    }

    public Triangle CalculateActualLocalViewTriangle() {
        Triangle loc = CalculateActualViewTriangle();
        return new Triangle(
                transform.InverseTransformPoint(loc.p1),
                transform.InverseTransformPoint(loc.p2),
                transform.InverseTransformPoint(loc.p3));
    }

    void OnDrawGizmos() {
        if (rightShadowEdge != null) {
            Gizmos.color = Color.magenta;
            foreach (var side in CalculateActualViewTriangle().GetSides()) {
                Gizmos.DrawLine(side.p1, side.p2);
            }
        }
        //foreach (var side in CalculateTargetViewTriangle().GetSides()) {
        //    Gizmos.DrawLine(side.p1, side.p2);
        //}
        //OnDrawGizmosSelected();
    }

    //void OnDrawGizmosSelected() {
    //    Gizmos.color = Color.green;
    //    foreach (var seg in trimmedShadows) {
    //        Gizmos.DrawLine(seg.p1, seg.p2);
    //    }
    //    Gizmos.color = Color.white;
    //}

    protected override void Awake() {
        base.Awake();

        shadowParent = new GameObject(gameObject.name + " shadows");

        rightShadowEdge = CreateLightEdge();
        leftShadowEdge = CreateLightEdge();
        farShadowEdge = CreateLightEdge();

        CacheViewTriangles();

        castLightMesh = Util.CreateMeshWithNewMaterial(gameObject, Refs.instance.lightMaterial);

        visibleCollider = gameObject.AddComponent<PolygonCollider2D>();
    }

    private LightEdge CreateLightEdge() {
        LightEdge edge = Util.CreateChild<LightEdge>(shadowParent.transform);
        edge.Init(this);
        return edge;
    }

    //void DrawLampshade() {
    //    Vector3 v1 = Vector3.zero;
    //    Vector3 v2 = Quaternion.Euler(0f,0f, angle/2) * Vector2.up;
    //    Vector3 v3 = Quaternion.Euler(0f,0f, -angle/2) * Vector2.up;
    //
    //    myMesh.vertices = new []{v1, v2, v3};
    //    myMesh.triangles = new []{0,1,2};
    //}

    void DrawCastedLight() {

        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        vertices.Add(GetActualPosition());

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

    public override void DoFixedUpdate() {
        CacheViewTriangles();

        DoShadows();
    }

    void Update() {
        DoShadows();
        DrawCastedLight();
    }

    void DoShadows() {
        UpdateKnownShadows();
        var shadowCorrespondences = new List<System.Tuple<LineSegment, Shadow>>();

        foreach (Shadow s in shadows.Values) {
            var i = s.caster.CrossSection(GetActualPosition()).Intersect(actualViewTriangle);
            if (i is LineSegment seg) {
                shadowCorrespondences.Add(System.Tuple.Create(seg, s));
            }
        }
        shadowCorrespondences.Add(System.Tuple.Create(ActualFarEdge(), (Shadow)null));

        MinimalUnion<Shadow>.Calculate(ref shadowCorrespondences, GetActualPosition(), Angle);

        shadowCorrespondences.Sort((s1, s2) => Angle(s1.Item1.Midpoint()).CompareTo(Angle(s2.Item1.Midpoint())));

        trimmedShadows.Clear();
        foreach (var tuple in shadowCorrespondences) {
            trimmedShadows.Add(tuple.Item1);
        }

        SendDataToShadows(shadowCorrespondences);
        SendDataToLightEdges(shadowCorrespondences);
    }

    void SendDataToShadows(List<System.Tuple<LineSegment, Shadow>> shadowData) {
        var frontFacing = new Dictionary<Shadow, List<LineSegment>>();
        var leftFacing = new Dictionary<Shadow, LineSegment>();
        var rightFacing = new Dictionary<Shadow, LineSegment>();

        for (int i = 0; i < shadowData.Count; i++) {
            LineSegment? prevSeg = i == 0 ? (LineSegment?)null : shadowData[i-1].Item1;
            LineSegment? nextSeg = i == shadowData.Count-1 ? (LineSegment?)null : shadowData[i+1].Item1;
            LineSegment seg = shadowData[i].Item1;
            Shadow shadow = shadowData[i].Item2;

            if (shadow != null) {
                if (!frontFacing.ContainsKey(shadow)) {
                    frontFacing.Add(shadow, new List<LineSegment>());
                }
                frontFacing[shadow].Add(seg);
                if (prevSeg is LineSegment prev) {
                    LineSegment right = new LineSegment(prev.p2, seg.p1);
                    if (right.Length() > .0001 && !right.GoesAwayFrom(GetActualPosition())) {
                        rightFacing[shadow] = right;
                    }
                }
                if (nextSeg is LineSegment next) {
                    LineSegment left = new LineSegment(seg.p2, next.p1);
                    if (left.Length() > .0001 && left.GoesAwayFrom(GetActualPosition())) {
                        leftFacing[shadow] = left;
                    }
                }
            }
        }

        foreach (var entry in frontFacing) {
            entry.Key.SetFrontEdges(entry.Value);
        }
        foreach (var entry in rightFacing) {
            entry.Key.SetRightEdge(entry.Value);
        }
        foreach (var entry in leftFacing) {
            entry.Key.SetLeftEdge(entry.Value);
        }
    }

    void SendDataToLightEdges(List<System.Tuple<LineSegment, Shadow>> shadowData) {
        rightShadowEdge.SetTarget(TargetRightEdge());
        leftShadowEdge.SetTarget(TargetLeftEdge());
        farShadowEdge.SetTarget(TargetFarEdge());

        var rightExtent = shadowData[0].Item1.p1;
        var leftExtent = shadowData[shadowData.Count-1].Item1.p2;
        rightShadowEdge.SetTargetLength((rightExtent - GetTargetPosition()).magnitude);
        leftShadowEdge.SetTargetLength((leftExtent - GetTargetPosition()).magnitude);

    }

    float Angle(Vector2 p) {
        return TargetRightEdge().Angle(p);
    }

    public override bool IsInDark(Vector2 point) {
        if (!this.actualViewTriangle.Contains(point)) {
            return true;
        }

        foreach (LineSegment seg in trimmedShadows) {
            if (new Triangle(GetActualPosition(), seg.p1, seg.p2).Contains(point)) {
                return false;
            }
        }
        return true;
    }

    void UpdateKnownShadows() {
        // TODO: This should be larger than the actual view triangle. This may
        // be why there are problems with update loops. Maybe its fine if we
        // can get the intersections without a physics iteration.
        visibleCollider.SetPath(0, CalculateActualLocalViewTriangle().AsList());

        // We use raycasts so that we get the current collisions without having
        // to run a physics step
        var raycastHits = new List<RaycastHit2D>();
        GetComponent<Rigidbody2D>().Cast(Vector2.up, raycastHits, 0);
        
        
        var opaqueSet = new HashSet<Opaque>();
        foreach (var hit in raycastHits) {
            foreach (var opaque in hit.collider.GetComponents<Opaque>()) {
                opaqueSet.Add(opaque);
            }
        }
    
        foreach (var opaque in opaqueSet) {
            if (!shadows.ContainsKey(opaque)) {
                Shadow s = Util.CreateChild<Shadow>(shadowParent.transform);
                s.Init(opaque, this);
                shadows.Add(opaque, s);
            }
        }
    
        HashSet<Opaque> toRemove = new HashSet<Opaque>();
        foreach (var opaque in shadows.Keys) {
            if (!opaqueSet.Contains(opaque)) {
                toRemove.Add(opaque);
            }
        }

        foreach (var opaque in toRemove) {
            Shadow s = shadows[opaque];
            Destroy(s.gameObject);
            shadows.Remove(opaque);
        }
    }
}
