using UnityEngine;
using System.Collections.Generic;

public abstract class DividesLight : AllTracker<DividesLight> {
    private static List<LineSegment> pieces = new List<LineSegment>();

    public enum Side {
        right,
        left
    }

    protected Rigidbody2D rb;
    [SerializeField]
    protected LineSegment target = LineSegment.zero;
    [SerializeField]
    private LightBase lightSource;
    // Says which side is illuminated by 'lightSource'
    [SerializeField]
    private Side illuminatedSide;

    // The line segment used to calculate intersections
    public abstract LineSegment GetDivider();

    protected virtual bool SegmentDividesLightAndDark(LineSegment seg) {
        var sideToCheck = illuminatedSide == Side.right ? seg.GetLeftSide() : seg.GetRightSide();

        foreach (var light in LightBase.GetAll()) {
            if (light != lightSource && !light.IsInDark(sideToCheck)) {
                return false;
            }
        }
        return true;
    }

    public void SetTarget(LineSegment target) {
        if (this.target == LineSegment.zero) {
            rb.position = target.p1;
            rb.rotation = target.Angle();
        }
        this.target = target;
    }

    public abstract void DoFixedUpdate();

    protected override void Awake() {
        base.Awake();
        gameObject.layer = PhysicsHelper.shadowEdgeLayer;
        rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.mass = 100;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void Init(LightBase lightSource, Side illuminatedSide) {
        Debug.Assert(!initialized);
        initialized = true;
        this.lightSource = lightSource;
        this.illuminatedSide = illuminatedSide;
    }

    private bool initialized = false;
    void Start() {
        Debug.Assert(initialized);
    }

    protected void AddSimpleForces() {
        if (target == LineSegment.zero) {
            return;
        }
        Vector2 targetCenterOfMass = target.p1 + (target.p2 - target.p1).normalized*rb.centerOfMass.magnitude;
        Vector2 force = PhysicsHelper.GetMoveToForce(rb, rb.worldCenterOfMass, targetCenterOfMass);
        float torque = PhysicsHelper.GetRotateToTorque(rb, target.Angle());

        //rb.AddForce(Vector2.ClampMagnitude(force, 10*rb.mass));
        //rb.AddTorque(Mathf.Clamp(torque, -10*rb.inertia, 10*rb.inertia));
        rb.AddForce(force);
        rb.AddTorque(torque);
    }

    protected void UpdateColliders() {
        GetDivider().Split(GetIntersectionCandidates(), in pieces);

        var colliders = new List<BoxCollider2D>(GetComponents<BoxCollider2D>());

        for (int i = pieces.Count; i < colliders.Count; i++) {
            colliders[i].enabled = false;
        }
        for (int i = colliders.Count; i < pieces.Count; i++) {
            var toAdd = gameObject.AddComponent<BoxCollider2D>();
            colliders.Add(toAdd);
        }

        for (int i = 0; i < pieces.Count; i++) {
            float width = pieces[i].Length();
            // TODO: Maybe we shouldn't use the target here (<-- obsolete comment for now)
            // TODO: I feel like there is something important I'm missing here
            colliders[i].offset = new Vector2((pieces[i].p1 - GetDivider().p1).magnitude + width/2, 0);
            colliders[i].size = new Vector2(width, .01f);
            colliders[i].enabled = SegmentDividesLightAndDark(pieces[i]);
        }
    }


    IEnumerable<LineSegment> GetIntersectionCandidates() {
        foreach (DividesLight edge in GetAll()) {
            // TODO: This can be optimized, if its a bottleneck
            if (edge.lightSource == this.lightSource) {
                continue;
            }
            yield return edge.GetDivider();
        }
    }

    // Where the collider actually is
    public LineSegment GetActual() {
        // TODO: Is it a problem that the actual always has the same length as the target?
        var targetUnrotated = Vector2.right*target.Length();
        var targetRotated = (Vector2)(Quaternion.Euler(0,0,target.Angle())*targetUnrotated);
        return new LineSegment(rb.position, rb.position + targetRotated);
    }

    protected void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        //Draw();
    }

    protected void OnDrawGizmosSelected() {
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
}
