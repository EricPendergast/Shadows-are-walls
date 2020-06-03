using UnityEngine;

[RequireComponent(typeof(TargetJoint2D))]
public class PositionableObject : MonoBehaviour, Positionable {
    TargetJoint2D joint;
    
    void Awake() {
        joint = GetComponent<TargetJoint2D>();
    }

    public void SetTargetPosition(Vector2 target) {
        joint.target = target;
    }

    public Vector2 GetTargetPosition() {
        return joint.target;
    }

    public Vector2 GetActualPosition() {
        return transform.position;
    }
}
