using UnityEngine;


public class AngleLever : Lever {

    [SerializeField]
    private float angleLeft = 0;
    [SerializeField]
    private float angleRight = 360;
    [SerializeField]
    private float position = 0;
    [SerializeField]
    private float speed = .1f;
    [SerializeField]
    private FixedLight controled;

    public void Update() {
        //position = controled.GetActualAngle() - angleLeft;
    }

    public override void MovePosition(int direction) {
        var deltaPosition = direction * speed / (angleLeft - angleRight);
        position = Mathf.Clamp(position + deltaPosition, 0, 1);
        controled.SetTargetAngle(Mathf.Lerp(angleLeft, angleRight, position));
    }
}
