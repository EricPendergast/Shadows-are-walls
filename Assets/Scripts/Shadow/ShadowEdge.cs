using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ShadowEdge : MonoBehaviour {
    private static Dictionary<int, ShadowEdge> allEdges = new Dictionary<int, ShadowEdge>();

    private List<LineSegment> pieces = new List<LineSegment>();
    // Used to cache the target, which is recalculated every FixedUpdate.
    [SerializeField]
    private LineSegment target = LineSegment.zero;
    private Rigidbody2D rb;
    [SerializeField]
    private Opaque caster;
    [SerializeField]
    private LightBase lightSource;

    protected void Awake() {
        allEdges.Add(gameObject.GetInstanceID(), this);
        gameObject.layer = PhysicsHelper.shadowEdgeLayer;
        rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.mass = 10;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private bool initialized = false;
    void Start() {
        Assert.IsTrue(initialized);
    }

    public void Init(Opaque caster, LightBase lightSource) {
        Assert.IsFalse(initialized);
        initialized = true;
        this.caster = caster;
        this.lightSource = lightSource;
    }

    public void SetTarget(LineSegment target) {
        if (this.target == LineSegment.zero) {
            rb.position = target.p1;
            rb.rotation = target.Angle();
        }
        this.target = target;
    }

    // Where the collider wants to be
    public LineSegment GetTarget() {
        return target;
    }

    // Where the collider actually is
    public LineSegment GetActual() {
        var targetUnrotated = Vector2.right*target.Length();
        var targetRotated = (Vector2)(Quaternion.Euler(0,0,target.Angle())*targetUnrotated);
        return new LineSegment(rb.position, rb.position + targetRotated);
    }

    // Where the collider reports its position as
    private LineSegment GetReported() {
        // caster being null indicates this is a light edge, which means its
        // actual position is the edge of the light
        if (caster == null) {
            return GetActual();
        }
        return GetTarget();
    }

    protected virtual void FixedUpdate() {
        //Debug.Log("ShadowEdge FixedUpdate");
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
        Gizmos.color = Color.red;
        Gizmos.DrawLine(target.p1, target.p2);
        //Gizmos.DrawSphere(target.p1, .1f);
        //Gizmos.DrawSphere(target.p2, .1f);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(target.p1, GetActual().p1);
        Gizmos.DrawLine(target.p2, GetActual().p2);
        //Gizmos.DrawSphere(transform.position, .1f);
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
        if (target == LineSegment.zero) {
            return;
        }
        Vector2 force = PhysicsHelper.GetMoveToForce(rb, target.p1);
        float torque = PhysicsHelper.GetRotateToTorque(rb, target.Angle());

        rb.AddForce(force);
        rb.AddTorque(torque);
    }

    void UpdateColliders() {
        GetReported().Split(GetIntersectionCandidates(), in pieces);

        var colliders = new List<BoxCollider2D>(GetComponents<BoxCollider2D>());

        for (int i = pieces.Count; i < colliders.Count; i++) {
            colliders[i].enabled = false;
        }
        for (int i = colliders.Count; i < pieces.Count; i++) {
            colliders.Add(gameObject.AddComponent<BoxCollider2D>());
        }

        for (int i = 0; i < pieces.Count; i++) {
            float width = pieces[i].Length();
            // TODO: Maybe we shouldn't use the target here
            colliders[i].offset = new Vector2((pieces[i].p1 - target.p1).magnitude + width/2, 0);
            colliders[i].size = new Vector2(width, .1f);
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
            yield return edge.GetReported();
        }
    }

    protected void OnDestroy() {
        allEdges.Remove(gameObject.GetInstanceID());
    }
}
