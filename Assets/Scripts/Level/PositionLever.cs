using UnityEngine;


public class PositionLever : Lever {

    [SerializeField]
    private PositionLeverPoint left;
    [SerializeField]
    private PositionLeverPoint right;
    [SerializeField]
    private float position = 0;
    [SerializeField]
    private float speed = .1f;
    [SerializeField]
    private GameObject controledGameObject;
    private Positionable controled;

    void Start() {
        controled = controledGameObject.GetComponent<Positionable>();
        position = ((Vector2)controled.GetActualPosition() - left.Position()).magnitude/(right.Position() - left.Position()).magnitude;
        MovePosition(0);
        left.myLever = this;
        right.myLever = this;
    }

    public void Update() {
        //position = ((Vector2)controled.GetActualPosition() - left).magnitude/(right - left).magnitude;
    }

    public override void MovePosition(int direction) {
        var deltaPosition = direction * speed / ((left.Position() - right.Position()).magnitude);
        position = Mathf.Clamp(position + deltaPosition, 0, 1);
        Debug.Log(position);
        controled.SetTargetPosition(Vector2.Lerp(left.Position(), right.Position(), position));
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.cyan;
        if (left != null && right != null) {
            Gizmos.DrawLine(left.Position(), right.Position());
        }
    }

    void OnDrawGizmosSelected() {
        if (left != null && right != null) {
            left.Draw();
            right.Draw();
        }
        Draw();
    }

    public void Draw() {
        Gizmos.color = Color.cyan;
        if (left != null && right != null) {
            Gizmos.DrawLine(left.Position(), right.Position());
        }
        if (controledGameObject != null) {
            GizmosUtil.DrawConstantWidthSphere(controledGameObject.GetComponent<Positionable>().GetActualPosition(), .05f);
        }
    }
}
