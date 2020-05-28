using System.Collections.Generic;
using UnityEngine;

public class ShadowEdge : MonoBehaviour {
    private static Dictionary<int, ShadowEdge> allEdges = new Dictionary<int, ShadowEdge>();

    private List<LineSegment> pieces = new List<LineSegment>();
    private List<Vector2> intersections = new List<Vector2>();
    // Used to cache the target, which is recalculated every FixedUpdate.
    [SerializeField]
    private LineSegment target = LineSegment.zero;
    private Rigidbody2D rb;

    protected void Awake() {
        allEdges.Add(gameObject.GetInstanceID(), this);
        gameObject.layer = LayerMask.NameToLayer("ShadowEdge");
        rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.mass = 1000000;
    }

    public void SetTarget(LineSegment target) {
        this.target = target;
        rb.transform.position = target.p1;
        rb.transform.rotation = Quaternion.Euler(0, 0, target.Angle());
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        //Draw();
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        DrawGizmos();
    }

    public void DrawGizmos() {
        Gizmos.DrawLine(target.p1, target.p2);
        //foreach (var seg in GetSplitOnIntersections(new List<LineSegment>())) {
        //    foreach (var point in new List<Vector2>{seg.GetRightSide(), seg.GetLeftSide()}) {
        //        if (LightBase.IsInDarkAllLights(point)) {
        //            Gizmos.color = Color.black;
        //        } else {
        //            Gizmos.color = Color.white;
        //        }
        //    
        //        Gizmos.DrawSphere(point, .1f);
        //    }
        //}

        //foreach (var i in GetIntersections(new List<Vector2>())) {
        //    Gizmos.DrawSphere(i, .1f);
        //}
        Gizmos.color = Color.white;
        //Gizmos.DrawSphere(target.p1, .1f);
    }

    void UpdateColliders() {
        GetSplitOnIntersections(in pieces);

        var colliders = new List<BoxCollider2D>(GetComponents<BoxCollider2D>());

        for (int i = pieces.Count; i < colliders.Count; i++) {
            colliders[i].enabled = false;
        }
        for (int i = colliders.Count; i < pieces.Count; i++) {
            colliders.Add(gameObject.AddComponent<BoxCollider2D>());
        }

        for (int i = 0; i < pieces.Count; i++) {
            float width = pieces[i].Length();
            colliders[i].offset = new Vector2((pieces[i].p1 - target.p1).magnitude + width/2, 0);
            colliders[i].size = new Vector2(width, .02f);
            colliders[i].enabled = SegmentDividesLightAndDark(pieces[i]);
        }
    }

    bool SegmentDividesLightAndDark(LineSegment seg) {
        return LightBase.IsInDarkAllLights(seg.GetRightSide()) != LightBase.IsInDarkAllLights(seg.GetLeftSide());
    }

    protected virtual void FixedUpdate() {
        UpdateColliders();
    }

    List<LineSegment> GetSplitOnIntersections(in List<LineSegment> splits) {
        splits.Clear();
        GetIntersections(in intersections);

        float totalLength = target.Length();
        intersections.Insert(0, target.p1);
        intersections.Add(target.p2);
        intersections.Sort((v1, v2) => DistAlongTarget(v1).CompareTo(DistAlongTarget(v2)));

        for (int i = 1; i < intersections.Count; i++) {
            splits.Add(new LineSegment(intersections[i-1], intersections[i]));
        }
        return splits;
    }

    float DistAlongTarget(Vector2 point) {
        return (target.p1 - point).sqrMagnitude;
    }

    List<Vector2> GetIntersections(in List<Vector2> intersections) {
        intersections.Clear();
        foreach (ShadowEdge edge in allEdges.Values) {
            if (edge == this) {
                continue;
            }
            if (target.Intersect(edge.target) is Vector2 intersec) {
                intersections.Add(intersec);
            }
        }
        return intersections;
    }

    protected void OnDestroy() {
        allEdges.Remove(gameObject.GetInstanceID());
    }
}
