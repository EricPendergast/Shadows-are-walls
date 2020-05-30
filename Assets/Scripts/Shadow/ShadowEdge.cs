using System.Collections.Generic;
using UnityEngine;

public class ShadowEdge : MonoBehaviour {
    private static Dictionary<int, ShadowEdge> allEdges = new Dictionary<int, ShadowEdge>();

    private List<LineSegment> pieces = new List<LineSegment>();
    // Used to cache the target, which is recalculated every FixedUpdate.
    [SerializeField]
    private LineSegment target = LineSegment.zero;
    private Rigidbody2D rb;

    protected void Awake() {
        allEdges.Add(gameObject.GetInstanceID(), this);
        gameObject.layer = PhysicsHelper.shadowEdgeLayer;
        rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.mass = 1000;
    }

    public void SetTarget(LineSegment target) {
        if (this.target == LineSegment.zero) {
            rb.transform.position = target.p1;
            rb.transform.rotation = Quaternion.Euler(0, 0, target.Angle());
        }
        this.target = target;
    }
    public LineSegment GetTarget() {
        return target;
    }

    protected virtual void FixedUpdate() {
        UpdateColliders();
        AddForces();
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

    void AddForces() {
        Vector2 force = PhysicsHelper.GetMoveToForce(rb, target.p1);
        float torque = PhysicsHelper.GetRotateToTorque(rb, target.Angle());

        rb.AddForce(force);
        rb.AddTorque(torque);
    }

    void UpdateColliders() {
        target.Split(GetIntersectionCandidates(), in pieces);

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

    IEnumerable<LineSegment> GetIntersectionCandidates() {
        foreach (ShadowEdge edge in allEdges.Values) {
            if (edge == this) {
                continue;
            }
            yield return edge.target;
        }
    }

    protected void OnDestroy() {
        allEdges.Remove(gameObject.GetInstanceID());
    }
}
