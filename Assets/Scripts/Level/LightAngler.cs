using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class LightAngler : LevelObject, SimpleButtonControlable, Interactable {

    [SerializeField]
    private bool DEBUG = false;
    [SerializeField]
    private bool unconstrained = false;
    [SerializeField]
    [Range(0,180)]
    private float angleConstraint = 90;
    [SerializeField]
    private float currentAngle = 0;
    [SerializeField]
    private float apertureAngle = 35;
    new private Rigidbody2D body {
        get => base.body == null ? GetComponent<Rigidbody2D>() : base.body;
    }
    [SerializeField]
    private float speed = 10;
    [SerializeField]
    public RotatableLight controled;
    private SimpleButton.State? buttonState = null;

    private RelativeJoint2D myJoint;

    public void Start() {
        ApplySettings();
        myJoint = gameObject.AddComponent<RelativeJoint2D>();
        myJoint.connectedBody = controled.GetComponent<Rigidbody2D>();
        myJoint.maxForce = 80;
        myJoint.maxTorque = 10;
        myJoint.autoConfigureOffset = false;
        currentAngle = 0;
        myJoint.angularOffset = currentAngle;

        Rotate(0);
    }

    public void Interact(Vector2 direction) {
        var lightDirection = controled.transform.right;

        if (direction == Vector2.zero || Vector2.Angle(direction, lightDirection) < 1) {
            return;
        }
        if (Vector2.SignedAngle(direction, lightDirection) < 0) {
            Rotate(1);
        } else {
            Rotate(-1);
        }
    }

    public Vector2 GetPosition() {
        return transform.position;
    }

    private void Rotate(int direction) {
        var delta = direction*speed*Time.deltaTime;
        var actualAngle = myJoint.connectedBody.rotation - body.rotation;

        currentAngle += delta;

        currentAngle = ClampToConstraints(currentAngle);
      
        currentAngle = Mathf.Clamp(currentAngle, actualAngle - Mathf.Abs(2*delta), actualAngle + Mathf.Abs(2*delta));

        myJoint.angularOffset = currentAngle;
    }

    public void SetState(SimpleButton.State state) {
        buttonState = state;
    }

    void FixedUpdate() {
        if (buttonState == SimpleButton.State.unpressed) {
            Rotate(-1);
        } else if (buttonState == SimpleButton.State.pressed) {
            Rotate(1);
        }
    }

    private void OnDrawGizmos() {
        DrawGizmos(1);
    }
    protected override void OnDrawGizmosSelected() {
        DrawGizmos(20);
    }
    void DrawGizmos(float gizmoLength) {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(body.position, Quaternion.Euler(0,0,body.rotation - angleConstraint)*Vector2.right*gizmoLength);
        Gizmos.DrawRay(body.position, Quaternion.Euler(0,0,body.rotation - (angleConstraint - apertureAngle))*Vector2.right*gizmoLength);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(body.position, Quaternion.Euler(0,0,body.rotation + angleConstraint)*Vector2.right*gizmoLength);
        Gizmos.DrawRay(body.position, Quaternion.Euler(0,0,body.rotation + (angleConstraint - apertureAngle))*Vector2.right*gizmoLength);
    }

    private float ClampToConstraints(float lightAngle) {
        if (!unconstrained) {
            return Mathf.Clamp(lightAngle, -(angleConstraint - apertureAngle/2), angleConstraint - apertureAngle/2);
        } else {
            return lightAngle;
        }
    }

    public void ApplySettings() {
        angleConstraint = Mathf.Max(angleConstraint, apertureAngle/2);
        currentAngle = ClampToConstraints(currentAngle);
        if (controled != null) {
            controled.transform.rotation = Quaternion.Euler(0,0,currentAngle)*transform.rotation;
            controled.SetTargetApertureAngle(apertureAngle);
            controled.transform.localPosition = Vector3.zero;
        }
    }

    public override void DoSnapping() {
        if (!Application.isPlaying) {
            if (base.snapToGrid) {
                SnapPosition();
                body.position = transform.position;
            }
        }
    }
}
