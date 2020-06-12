using UnityEngine;

public class TargetRotationTest : MonoBehaviour {
    private readonly float maxTorque = 10000000000;
    private readonly float maxAngularSpeed = 120;
    [SerializeField]
    float targetAngle;
    [SerializeField]
    Rigidbody2D controled;
    HingeJoint2D joint;

    void Awake() {
        joint = gameObject.AddComponent<HingeJoint2D>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedBody = controled.GetComponent<Rigidbody2D>();
        joint.connectedAnchor = Vector2.zero;
        joint.anchor = Vector2.zero;
        joint.useMotor = true;
        joint.motor = new JointMotor2D{maxMotorTorque = maxTorque, motorSpeed = 0};

        controled.centerOfMass = Vector2.zero;
    }

    void FixedUpdate() {
        var deltaAngle = targetAngle - controled.rotation;

        var motorSpeed = deltaAngle/Time.deltaTime*.5f;
        joint.motor = new JointMotor2D{maxMotorTorque = maxTorque, motorSpeed = Mathf.Clamp(motorSpeed, -maxAngularSpeed, maxAngularSpeed)};
    }
}
