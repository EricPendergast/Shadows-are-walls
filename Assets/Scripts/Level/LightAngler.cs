using UnityEngine;

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
    private float? _initialAngle;
    private float initialAngle {
        get => _initialAngle == null ? controled.GetActualAngle() - GetComponent<Rigidbody2D>().rotation: (float)_initialAngle;
    }
    [SerializeField]
    private float speed = 10;
    [SerializeField]
    private FixedLight controled;
    private SimpleButton.State? buttonState = null;
    [SerializeField]
    private float gizmoLength = 1;

    private RelativeJoint2D myJoint;

    public void Start() {
        _initialAngle = initialAngle;
        myJoint = gameObject.AddComponent<RelativeJoint2D>();
        myJoint.connectedBody = controled.GetComponent<Rigidbody2D>();
        myJoint.maxForce = 10;
        myJoint.maxTorque = 10;
        myJoint.autoConfigureOffset = false;
        currentAngle = 0;
        myJoint.angularOffset = initialAngle + currentAngle;

        MovePosition(0);
    }

    public void MovePosition(int direction) {
        var minAngle = Mathf.Min(-angleLeft, angleRight);
        var maxAngle = Mathf.Max(-angleLeft, angleRight);
        if (-angleLeft > angleRight) {
            direction = -direction;
        }

        var newCurrentAngle = Mathf.Clamp(currentAngle + direction*speed*Time.deltaTime, minAngle, maxAngle);

        //var targetAngle = initialAngle + newCurrentAngle;
        //
        //var actualAngle = controled.GetActualAngle() - GetComponent<Rigidbody2D>().rotation;
        //
        //var differenceFromTarget = Mathf.Abs(Mathf.DeltaAngle(actualAngle, targetAngle));
        //if (differenceFromTarget > 2*speed*Time.deltaTime) {
        //    if (Mathf.Abs(Mathf.DeltaAngle(actualAngle, currentAngle)) <= differenceFromTarget) {
        //        return;
        //    }
        //}
        currentAngle = newCurrentAngle;
        myJoint.angularOffset = initialAngle + currentAngle;
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
        Gizmos.color = Color.red;
        Gizmos.DrawRay(controled.GetActualPosition(), Quaternion.Euler(0,0,initialAngle - angleLeft - controled.GetTargetApertureAngle()/2)*Vector2.right*gizmoLength);
        Gizmos.DrawRay(controled.GetActualPosition(), Quaternion.Euler(0,0,initialAngle + angleRight + controled.GetTargetApertureAngle()/2)*Vector2.right*gizmoLength);

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(controled.GetActualPosition(), Quaternion.Euler(0,0,initialAngle - angleLeft + controled.GetTargetApertureAngle()/2)*Vector2.right*gizmoLength);
        Gizmos.DrawRay(controled.GetActualPosition(), Quaternion.Euler(0,0,initialAngle + angleRight - controled.GetTargetApertureAngle()/2)*Vector2.right*gizmoLength);
    }
}
