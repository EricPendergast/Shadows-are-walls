using UnityEngine;


public class AngleLever : Lever {

    [SerializeField]
    private float angleLeft = 0;
    [SerializeField]
    private float angleRight = 360;
    [SerializeField]
    private float position;
    [SerializeField]
    private FixedLight controled;

    public void Update() {
        position = (controled.GetActualAngle() - angleLeft)/(angleRight - angleLeft);
    }

    public override float GetPosition() {
        return position;
    }

    public override void MovePosition(float direction) {
        position = Mathf.Clamp01(position + direction);
        controled.SetTargetAngle(Mathf.Lerp(angleLeft, angleRight, position));
    }
}
