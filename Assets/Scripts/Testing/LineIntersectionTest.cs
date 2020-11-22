using UnityEngine;

public class LineIntersectionTest : MonoBehaviour {
    public Vector2 p1;
    public Vector2 p2;
    public Vector2 p3;
    public Vector2 p4;

    public Vector2 intersection;

    public float drawRadius = .25f;
    public float epsilon = 1;

    void Update() {
        if (MouseMove(ref p1)){}
        else if (MouseMove(ref p2)){}
        else if (MouseMove(ref p3)){}
        else {MouseMove(ref p4);}
    }

    bool MouseMove(ref Vector2 point) {
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButton(0)) {
            if ((point - worldPosition).magnitude < drawRadius) {
                point = worldPosition;
                return true;
            }
        }
        return false;
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(p1, drawRadius);
        Gizmos.DrawSphere(p2, drawRadius);
        Gizmos.DrawLine(p1, p2);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(p3, drawRadius);
        Gizmos.DrawSphere(p4, drawRadius);
        Gizmos.DrawLine(p3, p4);

        Gizmos.color = Color.green;

        if (LineSegmentLib.LineSegmentsIntersection(p1, p2, p3, p4, out intersection, epsilon)) {
            Gizmos.DrawSphere(intersection, drawRadius);
        }
    }
}
