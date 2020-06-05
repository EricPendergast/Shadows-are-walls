using UnityEngine;

public class Refs : MonoBehaviour {
    public static Refs instance;

    public Material lightMaterial;
    public Material shadowMaterial;
    public CameraTracker2D cameraTracker;

    void Awake() {
        instance = this;
    }
}
