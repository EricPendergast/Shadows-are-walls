using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Rigidbody2D))]
public partial class Positioner : MonoBehaviour, Interactable {
    [SerializeField]
    private Vector2 destination;
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
        if (Application.isPlaying) {
            Debug.Assert(controled != null);

            myJoint = gameObject.AddComponent<RelativeJoint2D>();

            myJoint.connectedBody = controled.GetComponent<Rigidbody2D>();
            myJoint.maxForce = 5000;
            myJoint.maxTorque = 5000;
            myJoint.autoConfigureOffset = false;
            myJoint.angularOffset = rotation;

            MoveRaw(0);
        }
    }

    public void Interact(Vector2 direction) {
        if (direction == Vector2.zero) {
            return;
        } 

        Vector2 origin = (Vector2)transform.position;

        if (Vector2.Dot(direction, destination - origin) > 0) {
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
        Vector2 origin = (Vector2)transform.position;

        var deltaPosition = direction * speed / ((origin - destination).magnitude) * Time.deltaTime;
        position = Mathf.Clamp(position + deltaPosition, 0, 1);
        var newTarget = Vector2.Lerp(origin, destination, position);
        myJoint.linearOffset = transform.InverseTransformPoint(newTarget);
        //myJoint.angularOffset = GetComponent<Rigidbody2D>().rotation - controled.rotation;
        //controled.SetTargetPosition(Vector2.Lerp(left.Position(), destination.Position(), position));
    }
}
