using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class LightAngler : LevelObject, SimpleLeverControlable, SimpleButtonControlable {

    [SerializeField]
    [Range(-180,180)]
    private float angleLeft = 90;
    [SerializeField]
    [Range(-180,180)]
    private float angleRight = 90;
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
    public FixedLight controled;
    private SimpleButton.State? buttonState = null;
    //[SerializeField]
    //private float gizmoLength = 1;

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

        MovePosition(0);
    }

    public void MovePosition(int direction) {
        var minAngle = Mathf.Min(-angleLeft, angleRight);
        var maxAngle = Mathf.Max(-angleLeft, angleRight);
        if (-angleLeft > angleRight) {
            direction = -direction;
        }

        var delta = direction*speed*Time.deltaTime;
        var actualAngle = controled.GetActualAngle() - body.rotation;
        var newCurrentAngle = Mathf.Clamp(currentAngle + delta, minAngle, maxAngle);

      
        newCurrentAngle = Mathf.Clamp(newCurrentAngle, actualAngle - Mathf.Abs(2*delta), actualAngle + Mathf.Abs(2*delta));

        currentAngle = newCurrentAngle;
        myJoint.angularOffset = currentAngle;
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

    private void OnDrawGizmos() {
        DrawGizmos(1);
    }
    protected override void OnDrawGizmosSelected() {
        DrawGizmos(20);
    }
    void DrawGizmos(float gizmoLength) {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(body.position, Quaternion.Euler(0,0,body.rotation - angleLeft - controled.GetTargetApertureAngle()/2)*Vector2.right*gizmoLength);
        Gizmos.DrawRay(body.position, Quaternion.Euler(0,0,body.rotation - angleLeft + controled.GetTargetApertureAngle()/2)*Vector2.right*gizmoLength);

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(body.position, Quaternion.Euler(0,0,body.rotation + angleRight - controled.GetTargetApertureAngle()/2)*Vector2.right*gizmoLength);
        Gizmos.DrawRay(body.position, Quaternion.Euler(0,0,body.rotation + angleRight + controled.GetTargetApertureAngle()/2)*Vector2.right*gizmoLength);
    }

    public void ApplySettings() {
        var minAngle = Mathf.Min(-angleLeft, angleRight);
        var maxAngle = Mathf.Max(-angleLeft, angleRight);
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);
        controled.transform.rotation = Quaternion.Euler(0,0,currentAngle)*transform.rotation;
        controled.SetTargetApertureAngle(apertureAngle);
        controled.transform.localPosition = Vector3.zero;
    }

    public override void DoSnapping() {
        if (!Application.isPlaying) {
            SnapPosition();
            body.position = transform.position;
        }
    }
}
