using UnityEngine;

public class PositionerPoint : SnappableObject {

    public Positioner myPositioner;

    void OnDrawGizmosSelected() {
        Draw();
        if (myPositioner != null) {
            myPositioner.Draw();
        }
    }

    public void Draw() {
        Gizmos.color = Color.white;
        GizmosUtil.DrawConstantWidthSphere(transform.position, .1f);
        if (myPositioner != null) {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(Position(), myPositioner.transform.position);
        }
    }

    public Vector2 Position() {
        return transform.position;
    }
}
