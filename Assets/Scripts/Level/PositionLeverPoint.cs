

using UnityEngine;


public class PositionLeverPoint : MonoBehaviour {

    public PositionLever myLever;

    void OnDrawGizmosSelected() {
        Draw();
        if (myLever != null) {
            myLever.Draw();
        }
    }

    public void Draw() {
        Gizmos.color = Color.white;
        GizmosUtil.DrawConstantWidthSphere(transform.position, .1f);
        if (myLever != null) {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(Position(), myLever.transform.position);
        }
    }

    public Vector2 Position() {
        return transform.position;
    }
}
