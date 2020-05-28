using System.Collections.Generic;
using UnityEngine;

// Idea: Light finds all opaque objects in
// the scene, calculates all their shadows,
// and then does rendering and collisions
// stuff
[RequireComponent(typeof(Rigidbody2D))]
public class FixedLight : LightBase {
    public float angle;
    public float distance;

    private Mesh castLightMesh;

    private PolygonCollider2D visibleCollider;

    private Dictionary<int, Shadow> shadows = new Dictionary<int, Shadow>();

    // All visible shadows. This does not contain Shadow objects because
    // sometimes shadows get split into multiple line segments
    private List<LineSegment> trimmedShadows = new List<LineSegment>();

    private Triangle viewTriangle;

    public void SetApertureAngle(float v) {
        angle = v;
    }

    public float GetApertureAngle() {
        return angle;
    }

    public float GetAngle() {
        return transform.localRotation.eulerAngles.z;
    }

    public void SetTargetAngle(float v) {
        transform.localRotation = Quaternion.Euler(0, 0, v);
    }

    public override Vector2 Position() {
        return transform.position;
    }

    public LineSegment FarEdge() {
        return new LineSegment(viewTriangle.p2, viewTriangle.p3);
    }

    public LineSegment RightEdge() {
        return new LineSegment(viewTriangle.p1, viewTriangle.p3);
    }

    public LineSegment LeftEdge() {
        return new LineSegment(viewTriangle.p1, viewTriangle.p2);
    }

    public Triangle LocalViewTriangle() {
        Vector3 v1 = Vector3.zero;
        Vector3 v2 = Quaternion.Euler(0f,0f, angle/2) * Vector2.up;
        Vector3 v3 = Quaternion.Euler(0f,0f, -angle/2) * Vector2.up;
        v2 *= distance;
        v3 *= distance;

        return new Triangle(v1, v2, v3);
    }

    public Triangle CalculateViewTriangle() {
        Triangle loc = LocalViewTriangle();
        return new Triangle(
                transform.TransformPoint(loc.p1),
                transform.TransformPoint(loc.p2),
                transform.TransformPoint(loc.p3));
    }

    void OnDrawGizmos() {
        foreach (var side in CalculateViewTriangle().GetSides()) {
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
        viewTriangle = CalculateViewTriangle();

        Util.CreateChild<CustomShadowEdge>(transform).Init(() => {
            return RightEdge();
        });
        Util.CreateChild<CustomShadowEdge>(transform).Init(() => {
            return LeftEdge();
        });
        Util.CreateChild<CustomShadowEdge>(transform).Init(() => {
            return FarEdge();
        });

        Refs.instance.lightMaterial.SetInt("lightId", lightCounter);
        Refs.instance.shadowMaterial.SetInt("lightId", lightCounter);
        lightCounter++;

        castLightMesh = Util.CreateMeshWithNewMaterial(gameObject, Refs.instance.lightMaterial);

        visibleCollider = gameObject.AddComponent<PolygonCollider2D>();
        visibleCollider.SetPath(0, viewTriangle.AsList());
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

    void FixedUpdate() {
        viewTriangle = CalculateViewTriangle();

        visibleCollider.SetPath(0, LocalViewTriangle().AsList());

        DoShadows();
        DrawCastedLight();


        //if (IsInDark(Mouse.WorldPosition())) {
        //    Debug.Log("Mouse in dark");
        //} else {
        //    Debug.Log("Mouse in light");
        //}
    }

    void DoShadows() {
        var shadowCorrespondences = new List<System.Tuple<LineSegment, Shadow>>();

        foreach (Shadow s in shadows.Values) {
            // This if statement will should only be false in some rare edge
            // cases, due to float precision
            if (s.caster.CrossSection(transform.position).Intersect(viewTriangle) is LineSegment seg) {
                shadowCorrespondences.Add(System.Tuple.Create(seg, s));
            }
        }
        shadowCorrespondences.Add(System.Tuple.Create(FarEdge(), (Shadow)null));

        MinimalUnion<Shadow>.Calculate(ref shadowCorrespondences, transform.position, Angle);

        shadowCorrespondences.Sort((s1, s2) => Angle(s1.Item1.Midpoint()).CompareTo(Angle(s2.Item1.Midpoint())));

        trimmedShadows.Clear();
        foreach (var tuple in shadowCorrespondences) {
            trimmedShadows.Add(tuple.Item1);
        }

        SendDataToShadows(shadowCorrespondences);
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
                    if (right.Length() > .0001 && !right.GoesAwayFrom(Position())) {
                        rightFacing[shadow] = right;
                    }
                }
                if (nextSeg is LineSegment next) {
                    LineSegment left = new LineSegment(seg.p2, next.p1);
                    if (left.Length() > .0001 && left.GoesAwayFrom(Position())) {
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

    float Angle(Vector2 p) {
        return RightEdge().Angle(p);
    }

    public override bool IsInDark(Vector2 point) {
        if (!this.visibleCollider.OverlapPoint(point)) {
            return true;
        }

        foreach (LineSegment seg in trimmedShadows) {
            if (new Triangle(transform.position, seg.p1, seg.p2).Contains(point)) {
                return false;
            }
        }
        return true;
    }

    void OnCollisionEnter2D(Collision2D collision) {
        foreach (Opaque opaque in collision.gameObject.GetComponents<Opaque>()) {
            Shadow s = Util.CreateChild<Shadow>(transform);
            s.Init(opaque, this);
            shadows.Add(opaque.GetInstanceID(), s);
        }
    }

    void OnCollisionExit2D(Collision2D collision) {
        foreach (Opaque opaque in collision.gameObject.GetComponents<Opaque>()) {
            Shadow s = shadows[opaque.GetInstanceID()];
            Destroy(s.gameObject);
            shadows.Remove(opaque.GetInstanceID());
        }
    }
}
