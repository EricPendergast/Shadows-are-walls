using UnityEngine;

// TODO: I believe this should be a ScriptableObject
public class Refs : MonoBehaviour {
    public static Refs instance;

    public Material lightMaterial;
    public Material shadowMaterial;
    public Material lampshadeMaterial;
    public CameraTracker2D cameraTracker;
    public PhysicsMaterial2D frictionlessMaterial;

    void Awake() {
        instance = this;
    }
}
