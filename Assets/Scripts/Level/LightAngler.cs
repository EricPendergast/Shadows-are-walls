using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Rigidbody2D))]
public class LightAngler : MonoBehaviour, SimpleLeverControlable, SimpleButtonControlable {

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
    private Rigidbody2D _body;
    private Rigidbody2D body {
        get => _body == null ? GetComponent<Rigidbody2D>() : _body;
    }
    [SerializeField]
    private float speed = 10;
    [SerializeField]
    public FixedLight controled;
    private SimpleButton.State? buttonState = null;
    [SerializeField]
    private float gizmoLength = 1;

    private RelativeJoint2D myJoint;

    public void Start() {
        if (Application.IsPlaying(gameObject)) {
            _body = body;
            myJoint = gameObject.AddComponent<RelativeJoint2D>();
            myJoint.connectedBody = controled.GetComponent<Rigidbody2D>();
            myJoint.maxForce = 40;
            myJoint.maxTorque = 40;
            myJoint.autoConfigureOffset = false;
            currentAngle = 0;
            myJoint.angularOffset = currentAngle;

            MovePosition(0);
        }
    }

    public void MovePosition(int direction) {
        var minAngle = Mathf.Min(-angleLeft, angleRight);
        var maxAngle = Mathf.Max(-angleLeft, angleRight);
        if (-angleLeft > angleRight) {
            direction = -direction;
        }

        var delta = direction*speed*Time.deltaTime;
        var actualAngle = myJoint.connectedBody.rotation - body.rotation;
        var newCurrentAngle = Mathf.Clamp(currentAngle + delta, minAngle, maxAngle);

      
        newCurrentAngle = Mathf.Clamp(newCurrentAngle, actualAngle - Mathf.Abs(2*delta), actualAngle + Mathf.Abs(2*delta));

        currentAngle = newCurrentAngle;
        myJoint.angularOffset = currentAngle;
    }

    public void SetState(SimpleButton.State state) {
        buttonState = state;
    }

    void FixedUpdate() {
        if (Application.IsPlaying(gameObject)) {
            if (buttonState == SimpleButton.State.unpressed) {
                MovePosition(-1);
            } else if (buttonState == SimpleButton.State.pressed) {
                MovePosition(1);
            }
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(controled.GetActualPosition(), Quaternion.Euler(0,0,body.rotation - angleLeft - controled.GetTargetApertureAngle()/2)*Vector2.right*gizmoLength);
        Gizmos.DrawRay(controled.GetActualPosition(), Quaternion.Euler(0,0,body.rotation + angleRight + controled.GetTargetApertureAngle()/2)*Vector2.right*gizmoLength);

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(controled.GetActualPosition(), Quaternion.Euler(0,0,body.rotation - angleLeft + controled.GetTargetApertureAngle()/2)*Vector2.right*gizmoLength);
        Gizmos.DrawRay(controled.GetActualPosition(), Quaternion.Euler(0,0,body.rotation + angleRight - controled.GetTargetApertureAngle()/2)*Vector2.right*gizmoLength);
    }


    public void Update() {
        if (!Application.IsPlaying(gameObject)) {
            currentAngle = Mathf.Clamp(currentAngle, -angleLeft, angleRight);
            controled.transform.rotation = Quaternion.Euler(0,0,currentAngle)*transform.rotation;
            controled.SetTargetApertureAngle(apertureAngle);
            controled.transform.localPosition = Vector3.zero;
        }
    }
}
