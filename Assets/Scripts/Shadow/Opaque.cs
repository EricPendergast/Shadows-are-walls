using UnityEngine;

//[RequireComponent(typeof(Rigidbody2D))]
public class Opaque : MonoBehaviour {
    public System.Func<Vector2, LineSegment> crossSectionCallback;
    // Indicates whether the front shadow edge face should have colliders. If
    // the opaque object already has a collider, this should be false
    public bool disableFrontFaceColliders;

    void Awake() {
        disableFrontFaceColliders = false;
    }

    public LineSegment? CrossSection(Vector2 cameraPos) {
        if (crossSectionCallback == null) {
            return null;
        } else {
            return crossSectionCallback(cameraPos);
        }
    }
}
