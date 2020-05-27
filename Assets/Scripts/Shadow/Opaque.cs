using UnityEngine;

public class Opaque : MonoBehaviour {
    public Vector2 p1;
    public Vector2 p2;

    void OnDrawGizmos() {
        Gizmos.DrawLine(CrossSection(Vector2.zero).p1, CrossSection(Vector2.zero).p2);
    }

    public LineSegment CrossSection(Vector2 cameraPos) {
        return new LineSegment(transform.TransformPoint(p1), transform.TransformPoint(p2));
    }
}
