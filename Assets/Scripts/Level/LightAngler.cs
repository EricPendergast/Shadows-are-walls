using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class LightAngler : LevelObject, Interactable {

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
    private float currentSpeed = 0;
    [SerializeField]
    public RotatableLight controled;

    private RelativeJoint2D myJoint;

    public void Start() {
        ApplySettings();
        myJoint = gameObject.AddComponent<RelativeJoint2D>();
        myJoint.connectedBody = controled.GetComponent<Rigidbody2D>();
        myJoint.maxForce = 80;
        myJoint.maxTorque = 10;
        myJoint.autoConfigureOffset = false;
        myJoint.angularOffset = currentAngle;

        Rotate(0);
    }

    public void Interact(Vector2 direction) {
        var lightDirection = controled.transform.right;

        if (direction == Vector2.zero || Vector2.Angle(direction, lightDirection) < 1) {
            return;
        }
        var angle = Vector2.SignedAngle(lightDirection, direction);
        if (Vector2.SignedAngle(lightDirection, direction) < 0) {
            Rotate(-1);
        } else {
            Rotate(1);
        }
    }

    public Vector2 GetPosition() {
        return transform.position;
    }

    private void Rotate(int direction) {
        if (DEBUG) {
            Debug.Log("In Rotate");
        }
        var delta = direction*speed*Time.deltaTime;
        var actualAngle = myJoint.connectedBody.rotation - body.rotation;

        currentAngle += delta;

        currentAngle = ClampToConstraints(currentAngle);

        float maxDiff = 2*speed*Time.deltaTime;
        if (direction == 0) {
            maxDiff = 0;
        }
      
        currentAngle = Mathf.Clamp(currentAngle, actualAngle - maxDiff, actualAngle + maxDiff);

        myJoint.angularOffset = currentAngle;
    }

    private void OnDrawGizmos() {
        DrawGizmos(1);
    }

    protected override void OnDrawGizmosSelected() {
        DrawGizmos(20);
    }

    void DrawGizmos(float gizmoLength) {
        if (unconstrained) {
            return;
        }
        Gizmos.color = Color.red;
        Gizmos.DrawRay(body.position, Quaternion.Euler(0,0,body.rotation - angleConstraint)*Vector2.right*gizmoLength);
        Gizmos.DrawRay(body.position, Quaternion.Euler(0,0,body.rotation - (angleConstraint - apertureAngle))*Vector2.right*gizmoLength);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(body.position, Quaternion.Euler(0,0,body.rotation + angleConstraint)*Vector2.right*gizmoLength);
        Gizmos.DrawRay(body.position, Quaternion.Euler(0,0,body.rotation + (angleConstraint - apertureAngle))*Vector2.right*gizmoLength);
    }

    private float ClampToConstraints(float lightAngle) {
        if (!unconstrained) {
            return Mathf.Clamp(lightAngle, -(angleConstraint - apertureAngle/2) - .1f, angleConstraint - apertureAngle/2 + .1f);
        } else {
            return lightAngle;
        }
    }

    public void ApplySettings() {
        angleConstraint = Mathf.Max(angleConstraint, apertureAngle/2);
        currentAngle = ClampToConstraints(currentAngle);
        if (controled != null) {
            SetControledAngle(currentAngle);
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

    private float GetControledAngle() {
        return controled.GetComponent<Rigidbody2D>().rotation;
    }

    private void SetControledAngle(float angle) {
        controled.GetComponent<Rigidbody2D>().rotation = body.rotation + angle;
        // When this is called in the editor, the transform doesn't update
        // automatically, so we need to change it manually.
        controled.transform.localRotation = Quaternion.Euler(0,0,angle);
    }
}
