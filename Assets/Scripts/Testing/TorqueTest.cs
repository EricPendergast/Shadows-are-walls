using UnityEngine;

public class TorqueTest : MonoBehaviour {
    public float rotation;
    public float delta = 1;
    public float projectedRotation = 0;

    public bool onlyApplyTorque = false;
    public float torqueApply;

    void OnDrawGizmosSelected() {
        var body = GetComponent<Rigidbody2D>();
        Vector2 currentRot = Quaternion.Euler(0,0,body.rotation)*Vector3.right;
        Vector2 targetRot = Quaternion.Euler(0,0,rotation)*Vector3.right;
        Vector2 projectedRot = Quaternion.Euler(0,0,projectedRotation)*Vector3.right;

        Gizmos.DrawRay(body.position, currentRot);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(body.position, targetRot);
        Gizmos.DrawSphere(body.position + targetRot, .1f);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(body.position, projectedRot);
        //Gizmos.DrawLine(GetComponent<Rigidbody2D>().position, projectedRotation);
    }

    void FixedUpdate() {
        var body = GetComponent<Rigidbody2D>();
        float torque;
        if (!onlyApplyTorque) {
            torque = PhysicsHelper.GetRotateToTorque(body, rotation);
            Debug.Log(torque);
            rotation += delta;
        } else {
            torque = torqueApply;
        }

        projectedRotation = body.rotation + body.angularVelocity*Time.deltaTime + .5f*(torque/body.inertia)*Time.deltaTime*Time.deltaTime;
        body.AddTorque(torque);
    }

}
