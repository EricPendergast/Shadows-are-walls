using UnityEngine;

public class ReachTargetTest : MonoBehaviour {

    public Vector2 p1;
    public Vector2 p2;

    private Rigidbody2D rb;
    private BoxCollider2D coll;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
    }

    void FixedUpdate() {
        LineSegment target = new LineSegment(p1, p2);
        float centerOfMassDistance = rb.centerOfMass.magnitude;
        Vector2 targetCenterOfMass = p1 + (p2 - p1).normalized*centerOfMassDistance;
        Vector2 force = PhysicsHelper.GetMoveToForce(rb, rb.worldCenterOfMass, targetCenterOfMass);
        float torque = PhysicsHelper.GetRotateToTorque(rb, target.Angle());

        rb.AddForce(force);
        rb.AddTorque(torque);
    }

    void OnDrawGizmos() {
        LineSegment target = new LineSegment(p1, p2);
        float centerOfMassDistance = (rb.worldCenterOfMass - rb.position).magnitude;
        Vector2 targetCenterOfMass = p1 + (p2 - p1).normalized*centerOfMassDistance;

        Gizmos.DrawLine(p1, p2);
        GizmosUtil.DrawConstantWidthSphere(targetCenterOfMass, .01f);
    }
}
