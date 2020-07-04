using UnityEngine;
using System.Collections.Generic;
using RigidbodyExtensions;

class PhysicsHelper {

    public static int shadowEdgeLayer = LayerMask.NameToLayer("ShadowEdge");
    public static int lightLayer = LayerMask.NameToLayer("Light");
    public static int opaqueLayer = LayerMask.NameToLayer("Opaque");
    public static int freeObjectLayer = LayerMask.NameToLayer("FreeObject");
    public static int interactableLayer = LayerMask.NameToLayer("Interactable");
    public static int glassLayer = LayerMask.NameToLayer("Glass");

    public static int shadowEdgeLayerMask = 2 << shadowEdgeLayer - 1;
    public static int lightLayerMask = 2 << lightLayer - 1;
    public static int opaqueLayerMask = 2 << opaqueLayer - 1;
    public static int freeObjectLayerMask = 2 << freeObjectLayer - 1;
    public static int interactableLayerMask = 2 << interactableLayer - 1;
    public static int glassLayerMask = 2 << glassLayer - 1;

    // Truncates 'force' so that when applied to 'body', it will not go over
    // maxSpeed in the next tick
    public static Vector3 TruncateForce(Rigidbody2D body, float maxSpeed, Vector3 force) {
        Vector2 accel = force/body.mass;
        
        Vector2 parallelVelocity = Vector3.Project(body.velocity, accel);
        
        Vector2 newVelocity = parallelVelocity + accel*Time.deltaTime;

        if (newVelocity.magnitude < maxSpeed) {
            return force;
        }

        if (newVelocity == Vector2.zero) {
            newVelocity = accel.normalized*.0001f;
        }
        newVelocity = newVelocity.normalized * maxSpeed;
        
        return GetNeededForce(body, parallelVelocity, newVelocity);

        // TODO: There is an assumption here; this is right most of the time,
        // but if force is large and maxSpeed is small, this will break
    }

    public static Vector2 GetNeededForce(Rigidbody2D body, Vector2 endVelocity) {
        return GetNeededForce(body, body.velocity, endVelocity);
    }
    // Gives the force needed for 'body' to go from startVelocity to endVelocity in the next tick
    public static Vector2 GetNeededForce(Rigidbody2D body, Vector2 startVelocity, Vector2 endVelocity) {
        return ((endVelocity - startVelocity)*body.mass)/Time.deltaTime;
    }

    public static float GetNeededTorque(Rigidbody2D body, float startAngVel, float endAngVel) {
        return ((endAngVel - startAngVel)*body.inertia)/Time.deltaTime;
    }
    public static float GetNeededTorque(Rigidbody2D body, float endAngVel) {
        return GetNeededTorque(body, body.angularVelocity, endAngVel);
    }

    public static float GetRotateToTorque(Rigidbody2D body, float startAngle, float endAngle) {
        return GetRotateToTorque(body, endAngle - startAngle + body.rotation);
    }
    public static float GetRotateToTorque(Rigidbody2D body, float endAngle) {
        float startAngle = body.rotation;
        // Making endAngle be as close to startAngle as possible (in
        // numerical distance, not angular distance; angular distance
        // stays the same)
        endAngle = startAngle + AngleDifference(startAngle, endAngle);
        float startAngVelocity = body.angularVelocity;
        float time = Time.deltaTime == 0 ? Time.fixedDeltaTime : Time.deltaTime;

        // Solving for angular acceleration using
        // p_t = p_0 + v_0*t + 1/2*a*t^2
        float angAccel = 2*(endAngle - startAngle - startAngVelocity*time)/(time*time);

        // For some strange reason, we need to divide by 115; I can't figure out why.
        return angAccel*body.inertia/115;
    }

    public static Vector2 GetMoveToForce(Rigidbody2D body, Vector2 endPosition) {
        return GetMoveToForce(body, body.position, endPosition);
    }
    public static Vector2 GetMoveToForce(Rigidbody2D body, Vector2 startPosition, Vector2 endPosition) {
        Vector2 startVelocity = body.velocity;
        float time = Time.deltaTime == 0 ? Time.fixedDeltaTime : Time.deltaTime;
        // Solving for acceleration using
        // p_t = p_0 + v_0*t + 1/2*a*t^2
        
        Vector2 accel = 2*(endPosition - startPosition - startVelocity*time)/(time*time);


        // For some strange reason, we need to divide by 2; I can't figure out why.
        return accel*body.mass/2;
    }

    // Calculates the change in angle from angle1 to angle2, in the
    // range [-180, 180]
    public static float AngleDifference(float angle1, float angle2 )
    {
        float diff = ( angle2 - angle1 + 180 ) % 360 - 180;
        return diff < -180 ? diff + 360 : diff;
    }

    public static void GetForceAndTorque(Rigidbody2D body, LineSegment target, out Vector2 force, out float torque) {
        Vector2 targetCenterOfMass = target.p1 + (target.p2 - target.p1).normalized*body.centerOfMass.magnitude;
        force = PhysicsHelper.GetMoveToForce(body, body.worldCenterOfMass, targetCenterOfMass);
        torque = PhysicsHelper.GetRotateToTorque(body, target.Angle());
    }

    public static void GetForceAndTorque(Rigidbody2D body, LineSegment actual, LineSegment target, out Vector2 force, out float torque) {
        target = target.Rotate(body.rotation - actual.Angle());
        target = target + (body.position + actual.p1);
        GetForceAndTorque(body, target, out force, out torque);
    }

    public static float GetSpringTorque(float rbAngle, float springAngle, float rbAngVel, float springAngVel, float springConstant, float damping) {
        // Using the equation F = -k*x - b*v
        var angleDifference = rbAngle - springAngle;
        return -angleDifference * springConstant - (rbAngVel - springAngVel)*damping;
    }

    public static float GetInertia(Rigidbody2D body, List<BoxCollider2D> boxes, Vector2 pivot) {
        float totalArea = 0;
        foreach (var box in boxes) {
            if (box.enabled) {
                totalArea += box.size.x*box.size.y;
            }
        }

        float inertia = 0;
        foreach (var box in boxes) {
            if (box.enabled) {
                var boxMass = (body.mass / totalArea) * box.size.x*box.size.y;
                // Parallel axis theorem
                var inertiaAroundCentroid = boxMass * (box.size.y*box.size.y + box.size.x*box.size.x) / 12;
                inertia += inertiaAroundCentroid + boxMass*box.offset.sqrMagnitude;
            }
        }

        return inertia;
    }
}
