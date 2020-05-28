using UnityEngine;
using System.Collections.Generic;

class PhysicsHelper {

    public static int shadowEdgeLayer = LayerMask.NameToLayer("ShadowEdge");
    public static int lightLayer = LayerMask.NameToLayer("Light");
    public static int opaqueLayer = LayerMask.NameToLayer("Opaque");
    public static int freeObjectLayer = LayerMask.NameToLayer("FreeObject");
    public static int interactableLayer = LayerMask.NameToLayer("Interactable");

    // Truncates 'force' so that when applied to 'body', it will no go over
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
        return ((endVelocity - body.velocity)*body.mass)/Time.deltaTime;
    }
    // Gives the force needed for 'body' to go from startVelocity to endVelocity in the next tick
    public static Vector2 GetNeededForce(Rigidbody2D body, Vector2 startVelocity, Vector2 endVelocity) {
        return ((endVelocity - startVelocity)*body.mass)/Time.deltaTime;
    }
}
