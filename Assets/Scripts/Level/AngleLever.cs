using UnityEngine;


public class AngleLever : Lever {

    [SerializeField]
    [Range(0,180)]
    private float angleLeft = 90;
    [SerializeField]
    [Range(0,180)]
    private float angleRight = 90;
    [SerializeField]
    private float position = 0;
    private float? initialAngle;
    [SerializeField]
    private float speed = .1f;
    [SerializeField]
    private FixedLight controled;

    public void Start() {
        //var actualAngle = (controled.GetActualAngle()%360+360)%360;
        position = 0;
        initialAngle = controled.GetActualAngle();
        //position = (actualAngle - angleLeft)/(angleRight-angleLeft);
    }

    public void Update() {
        //position = controled.GetActualAngle() - angleLeft;
    }

    public override void MovePosition(int direction) {
        //var deltaPosition;
        //if (direction < 0) {
        //    deltaPosition = direction * speed / angleLeft;
        //} else if (direction > 0) {
        //    deltaPosition = direction * speed / angleRight;
        //} else {
        //    deltaPosition = 0;
        //}
        //position = Mathf.Clamp(position + deltaPosition, -1, 1);
        position = Mathf.Clamp(position + direction*speed, -angleLeft, angleRight);
        controled.SetTargetAngle((float)initialAngle + position);
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
