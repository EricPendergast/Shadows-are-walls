using UnityEngine;

public class Refs : MonoBehaviour {
    public static Refs instance;

    public Material lightMaterial;
    public Material shadowMaterial;
    public CameraTracker2D cameraTracker;
    public PhysicsMaterial2D frictionlessMaterial;

    void Awake() {
        instance = this;
    }
}
