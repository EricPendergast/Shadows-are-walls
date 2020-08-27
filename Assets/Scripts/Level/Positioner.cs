using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Positioner : SnappableObject, Interactable {

    public Vector2 left {
        get => transform.position;
        set => transform.position = value;
    }

    [SerializeField]
    public Vector2 right;
    //public Vector2 right {
    //    get => transform.TransformPoint(rightLocal);
    //    set => rightLocal = transform.InverseTransformPoint(value);
    //}
    [SerializeField]
    private float position = 0;
    [SerializeField]
    private float speed = 3f;
    [SerializeField]
    private Rigidbody2D controled;
    [SerializeField]
    private float rotation;

    private RelativeJoint2D myJoint;

    void Start() {
        Debug.Assert(controled != null);

        myJoint = gameObject.AddComponent<RelativeJoint2D>();

        myJoint.connectedBody = controled.GetComponent<Rigidbody2D>();
        myJoint.maxForce = 100000;
        myJoint.maxTorque = 100000;
        myJoint.autoConfigureOffset = false;
        myJoint.angularOffset = rotation;

        MoveRaw(0);
    }

    public void Interact(Vector2 direction) {
        if (direction == Vector2.zero) {
            return;
        } 

        if (Vector2.Dot(direction, right - left) > 0) {
            MoveRaw(1);
        } else {
            MoveRaw(-1);
        }
    }

    public Vector2 GetPosition() {
        // This makes it so direct interaction is impossible. Theoretically,
        // this method should never even get called, since this object doesn't
        // have a collider.
        return Vector2.positiveInfinity;
    }

    private void MoveRaw(int direction) {
        var deltaPosition = direction * speed / ((left - right).magnitude) * Time.deltaTime;
        position = Mathf.Clamp(position + deltaPosition, 0, 1);
        var newTarget = Vector2.Lerp(left, right, position);
        myJoint.linearOffset = transform.InverseTransformPoint(newTarget);
        //myJoint.angularOffset = GetComponent<Rigidbody2D>().rotation - controled.rotation;
        //controled.SetTargetPosition(Vector2.Lerp(left.Position(), right.Position(), position));
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(left, right);
    }

    //void OnDrawGizmosSelected() {
    //    if (left != null && right != null) {
    //        left.Draw();
    //        right.Draw();
    //    }
    //    Draw();
    //}

    //public void Draw() {
    //    Gizmos.color = Color.cyan;
    //    if (left != null && right != null) {
    //        Gizmos.DrawLine(left, right);
    //    }
    //}

    public override void DoSnapping() {
        if (snapToGrid) {
            left = Snap(left);
            right = Snap(right);
        }
    }

    public void EditorUpdate() {
        position = Mathf.Clamp01(position);
        if (controled != null) {
            controled.transform.position = Vector2.Lerp(left, right, position);
            controled.transform.localRotation = Quaternion.Euler(0,0,rotation);
        }
    }
}
