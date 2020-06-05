using UnityEngine;

public class LightAngler : MonoBehaviour, SimpleLeverControlable {

    [SerializeField]
    [Range(0,180)]
    private float angleLeft = 90;
    [SerializeField]
    [Range(0,180)]
    private float angleRight = 90;
    [SerializeField]
    private float currentAngle = 0;
    private float? initialAngle;
    [SerializeField]
    private float speed = .1f;
    [SerializeField]
    private FixedLight controled;

    public void Start() {
        //var actualAngle = (controled.GetActualAngle()%360+360)%360;
        currentAngle = 0;
        initialAngle = controled.GetActualAngle();
        //currentAngle = (actualAngle - angleLeft)/(angleRight-angleLeft);
    }

    public void Update() {
        //currentAngle = controled.GetActualAngle() - angleLeft;
    }

    public void MovePosition(int direction) {
        //var deltaPosition;
        //if (direction < 0) {
        //    deltaPosition = direction * speed / angleLeft;
        //} else if (direction > 0) {
        //    deltaPosition = direction * speed / angleRight;
        //} else {
        //    deltaPosition = 0;
        //}
        //currentAngle = Mathf.Clamp(currentAngle + deltaPosition, -1, 1);
        currentAngle = Mathf.Clamp(currentAngle + direction*speed, -angleLeft, angleRight);
        // TODO: Don't do this, do something with joints
        controled.SetTargetAngle((float)initialAngle + currentAngle);
    }


    void OnDrawGizmos() {
        float? init = initialAngle;
        if (init == null) {
            init = controled.GetActualAngle();
        }
        Gizmos.color = Color.red;
        Gizmos.DrawRay(controled.GetActualPosition(), Quaternion.Euler(0,0,(float)init - angleLeft - controled.GetTargetApertureAngle()/2)*Vector2.right);
        Gizmos.DrawRay(controled.GetActualPosition(), Quaternion.Euler(0,0,(float)init + angleRight + controled.GetTargetApertureAngle()/2)*Vector2.right);
    }
}
