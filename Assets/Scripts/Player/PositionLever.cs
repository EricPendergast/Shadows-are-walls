using UnityEngine;


public class PositionLever : Lever {

    [SerializeField]
    private Vector2 positionLeft;
    [SerializeField]
    private Vector2 positionRight;
    [SerializeField]
    private float position = 0;
    [SerializeField]
    private FixedLight controled;

    public void Update() {
        //position = ((Vector2)controled.GetActualPosition() - positionLeft).magnitude/(positionRight - positionLeft).magnitude;
    }


    void OnDrawGizmosSelected() {
        Gizmos.DrawLine(positionLeft, positionRight);
    }
    public override float GetPosition() {
        return position;
    }

    public override void MovePosition(float direction) {
        position = Mathf.Clamp01(position + direction);
        controled.SetTargetPosition(Vector2.Lerp(positionLeft, positionRight, position));
    }
}
