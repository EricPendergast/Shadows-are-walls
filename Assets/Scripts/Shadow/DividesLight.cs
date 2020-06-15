using UnityEngine;
using System.Collections.Generic;

public abstract class DividesLight : AllTracker<DividesLight> {
    [SerializeField]
    protected bool DEBUG = false;

    private static List<LineSegment> pieces = new List<LineSegment>();

    private readonly float maxTorque = 1000000000;
    private readonly float maxAngularSpeed = 120;

    public enum Side {
        right,
        left
    }

    protected Rigidbody2D rb;
    [SerializeField]
    protected LineSegment target = LineSegment.zero;
    [SerializeField]
    protected LightBase lightSource;
    // Says which side is illuminated by 'lightSource'
    [SerializeField]
    private Side illuminatedSide;
    private Side initialIlluminatedSide;
    private HingeJoint2D joint;

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
        bool firstSetTarget = this.target == LineSegment.zero;

        this.target = target;

        if (!this.target.GoesAwayFrom(lightSource.GetTargetPosition())) {
            this.target = this.target.Swapped();
            illuminatedSide = initialIlluminatedSide == Side.left ? Side.right : Side.left;
        }

        if (firstSetTarget) {
            rb.position = lightSource.GetTargetPosition();
            rb.rotation = this.target.Angle();
            firstSetTarget = false;
        }
    }

    public abstract void DoFixedUpdate();

    protected override void Awake() {
        base.Awake();
        gameObject.layer = PhysicsHelper.shadowEdgeLayer;
        rb = gameObject.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.mass = 5;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void Init(LightBase lightSource, Side illuminatedSide) {
        Debug.Assert(!initialized);
        initialized = true;
        this.lightSource = lightSource;
        this.initialIlluminatedSide = illuminatedSide;
        this.illuminatedSide = illuminatedSide;

        joint = lightSource.GetEdgeMountPoint().gameObject.AddComponent<HingeJoint2D>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody = rb;
        joint.connectedAnchor = Vector2.zero;
        joint.useMotor = true;
        joint.motor = new JointMotor2D{maxMotorTorque = maxTorque, motorSpeed = 0};
    }

    private bool initialized = false;
    void Start() {
        Debug.Assert(initialized);
    }

    protected void AddSimpleForces() {
        if (DEBUG) {
            Debug.Log("Break point");
        }
        if (target == LineSegment.zero) {
            return;
        }
        var targetAngle = target.Angle();
        var deltaAngle = Mathf.DeltaAngle(rb.rotation, targetAngle);

        var motorSpeed = deltaAngle/Time.deltaTime;
        if (DEBUG) {
            Debug.Log("Motor speed: " + motorSpeed);
            Debug.Log("Target angle: " + targetAngle);
            Debug.Log("Delta angle: " + targetAngle);
        }
        joint.motor = new JointMotor2D{maxMotorTorque = maxTorque, motorSpeed = Mathf.Clamp(motorSpeed, -maxAngularSpeed, maxAngularSpeed)};
        //joint.connectedAnchor = new Vector2(-(lightSource.GetComponent<Rigidbody2D>().position - target.p1).magnitude, 0);
        //PhysicsHelper.GetForceAndTorque(rb, target, out Vector2 force, out float torque);

        //rb.AddForce(Vector2.ClampMagnitude(force, 10*rb.mass));
        //rb.AddTorque(Mathf.Clamp(torque, -10*rb.inertia, 10*rb.inertia));
        //rb.AddForce(force);
        //rb.AddTorque(torque);
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
            colliders[i].offset = new Vector2((pieces[i].p1 - lightSource.GetTargetPosition()).magnitude + width/2, 0);
            colliders[i].size = new Vector2(width, .01f);
            colliders[i].enabled = SegmentDividesLightAndDark(pieces[i]);
        }

        // Moving the center of mass to rb.position, which is where the light
        // source is. This makes things act nicer, for unclear reasons. But the
        // code in AddSimpleForces totally breaks if this doesn't happen.
        // Also, the inertia needs to be set manually because sometimes the
        // physics system doesn't update the inertia.
        rb.centerOfMass = Vector2.zero;
        rb.inertia = PhysicsHelper.GetInertia(rb, colliders, Vector2.zero);
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
        var targetRotated = (Vector2)(Quaternion.Euler(0,0,rb.rotation)*targetUnrotated);

        var actualP1 = rb.position + (Vector2)(targetRotated.normalized*((target.p1 - lightSource.GetTargetPosition()).magnitude));
        return new LineSegment(actualP1, actualP1 + targetRotated);
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
