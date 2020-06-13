using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Positioner : MonoBehaviour, SimpleLeverControlable, SimpleButtonControlable {

    [SerializeField]
    private PositionerPoint left;
    [SerializeField]
    private PositionerPoint right;
    [SerializeField]
    private float position = 0;
    [SerializeField]
    private float speed = 3f;
    [SerializeField]
    private Rigidbody2D controled;

    private RelativeJoint2D myJoint;
    [SerializeField]
    private SimpleButton.State? buttonState = null;

    void Start() {
        Debug.Assert(controled != null);
        position = (controled.position - left.Position()).magnitude/(right.Position() - left.Position()).magnitude;
        left.myPositioner = this;
        right.myPositioner = this;

        myJoint = gameObject.AddComponent<RelativeJoint2D>();

        myJoint.connectedBody = controled.GetComponent<Rigidbody2D>();
        myJoint.maxForce = 100000;
        myJoint.maxTorque = 100000;
        myJoint.autoConfigureOffset = false;
        myJoint.angularOffset = 0;

        MovePosition(0);
    }

    public void MovePosition(int direction) {
        var deltaPosition = direction * speed / ((left.Position() - right.Position()).magnitude) * Time.deltaTime;
        position = Mathf.Clamp(position + deltaPosition, 0, 1);
        var newTarget = Vector2.Lerp(left.Position(), right.Position(), position);
        myJoint.linearOffset = transform.InverseTransformPoint(newTarget);
        //myJoint.angularOffset = GetComponent<Rigidbody2D>().rotation - controled.rotation;
        //controled.SetTargetPosition(Vector2.Lerp(left.Position(), right.Position(), position));
    }

    public void SetState(SimpleButton.State state) {
        buttonState = state;
    }

    void FixedUpdate() {
        if (buttonState == SimpleButton.State.unpressed) {
            MovePosition(-1);
        } else if (buttonState == SimpleButton.State.pressed) {
            MovePosition(1);
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.cyan;
        if (left != null && right != null) {
            Gizmos.DrawLine(left.Position(), right.Position());
        }
    }

    void OnDrawGizmosSelected() {
        if (left != null && right != null) {
            left.Draw();
            right.Draw();
        }
        Draw();
    }

    public void Draw() {
        Gizmos.color = Color.cyan;
        if (left != null && right != null) {
            Gizmos.DrawLine(left.Position(), right.Position());
        }
        if (controled != null) {
            GizmosUtil.DrawConstantWidthSphere(controled.position, .05f);
        }
    }
}
