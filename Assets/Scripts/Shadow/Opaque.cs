using UnityEngine;

//[RequireComponent(typeof(Rigidbody2D))]
public class Opaque : MonoBehaviour {
    public System.Func<Vector2, LineSegment> crossSectionCallback;

    // TODO: Make this have nullable return value?
    public LineSegment CrossSection(Vector2 cameraPos) {
        if (crossSectionCallback == null) {
            return LineSegment.zero;
        } else {
            return crossSectionCallback(cameraPos);
        }
    }
}
