using UnityEngine;


public class PositionLever : Lever {

    [SerializeField]
    private Vector2 positionLeft;
    [SerializeField]
    private Vector2 positionRight;
    [SerializeField]
    private float position = 0;
    [SerializeField]
    private float speed = .1f;
    [SerializeField]
    private FixedLight controled;

    public void Update() {
        //position = ((Vector2)controled.GetActualPosition() - positionLeft).magnitude/(positionRight - positionLeft).magnitude;
    }


    void OnDrawGizmosSelected() {
        Gizmos.DrawLine(positionLeft, positionRight);
    }

    public override void MovePosition(int direction) {
        var deltaPosition = direction * speed / ((positionLeft - positionRight).magnitude);
        position = Mathf.Clamp(position + deltaPosition, 0, 1);
        Debug.Log(position);
        controled.SetTargetPosition(Vector2.Lerp(positionLeft, positionRight, position));
    }
}
