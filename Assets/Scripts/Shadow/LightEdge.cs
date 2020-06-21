using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightEdge : DividesLight {
    protected override void Awake() {
        base.Awake();

        //StartCoroutine(PostFixedUpdateLoop());
    }

    public void SetTargetLength(float newLength) {
        target = target.WithLength(newLength);
    }

    public override LineSegment GetDivider() {
        //return GetActual();
        return target;
    }

    //IEnumerator PostFixedUpdateLoop() {
    //    while (true) {
    //        yield return new WaitForFixedUpdate();
    //
    //    }
    //}
    //
    public override void DoFixedUpdate() {
        UpdateColliders();
        AddSimpleForces();
    }

    public float AngularVelocity() {
        return rb.angularVelocity;
    }

    // 'point' lies on the target
    // point+difference is the point on the actual light edge
    public void MaxDifferenceFromTarget(out Vector2 point, out Vector2 difference) {
        var contacts = new List<ContactPoint2D>();
        rb.GetContacts(contacts);

        point = Vector2.zero;
        difference = Vector2.zero;

        var actual = GetActual();

        foreach (var contact in contacts) {
            var targetClosest = target.Closest(contact.point);
            var thisDifference = actual.Closest(contact.point) - targetClosest;
            if (thisDifference.sqrMagnitude > difference.sqrMagnitude) {
                difference = thisDifference;
                difference -= ((Vector2)Vector3.Project(contact.normal, difference).normalized)*contact.separation;
                point = targetClosest;
            }
        }
    }

    public float AngularDifferenceFromTarget() {
        float maxDifference = 0;
        var contacts = new List<ContactPoint2D>();
        rb.GetContacts(contacts);

        foreach (var contact in contacts) {
            var angle = target.Angle(contact.point);
            if (Mathf.Abs(angle) > Mathf.Abs(maxDifference)) {
                maxDifference = angle;
            }
        }
        return maxDifference;
    }

    //void OnCollisionEnter2D(Collision2D collision) {
    //    DoCollision(collision);
    //    Debug.Log("Collision enter");
    //}
    //void OnCollisionExit2D(Collision2D collision) {
    //    DoCollision(collision);
    //    Debug.Log("Collision exit");
    //}
    //void OnCollisionStay2D(Collision2D collision) {
    //    DoCollision(collision);
    //    Debug.Log("Collision stay");
    //}
    //
    //void DoCollision(Collision2D collision) {
    //    var lightBody = lightSource.GetComponent<Rigidbody2D>();
    //    foreach (var contact in collision.contacts) {
    //        var forceToApply = -Time.deltaTime*contact.normal*contact.normalImpulse*.1f;
    //        Debug.Log("Adding force: " + forceToApply);
    //        lightBody.AddForceAtPosition(contact.point, forceToApply);
    //    }
    //}

    //void OnCollisionStay2D(Collision2D collision) {
    //    //float totalTorque = 0;
    //    //foreach (var contact in collision.contacts) {
    //    //    totalTorque += target.PerpendicularComponent(contact.normal*contact.normalImpulse).magnitude * (target.p1 - contact.point).magnitude;
    //    //}
    //    float? max = null;
    //    Vector2 penetrationPoint = Vector2.zero;
    //    Vector2 totalForce = Vector2.zero;
    //
    //    foreach (var contact in collision.contacts) {
    //        if (max == null || contact.separation > max) {
    //            max = contact.separation;
    //            penetrationPoint = contact.point;
    //        }
    //        totalForce += contact.normal*contact.normalImpulse;
    //    }
    //
    //    // TODO: I believe this will break if the light penetrates multiple objects
    //    if (max is float maxPenetration) {
    //        Debug.Log("Max penetration: " + maxPenetration);
    //        maxPenetration *= .5f;
    //        var resolvePoint = -target.PerpendicularComponent(totalForce).normalized*maxPenetration + penetrationPoint;
    //        
    //        var lightRb = lightSource.GetComponent<Rigidbody2D>();
    //        var lightTorque = PhysicsHelper.GetRotateToTorque(lightRb, 0, new LineSegment(lightRb.position, penetrationPoint).Angle(resolvePoint));
    //
    //        lightRb.AddTorque(lightTorque);
    //    }
    //}
}
