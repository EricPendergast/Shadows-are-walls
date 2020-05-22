using UnityEngine;

public class Refs : MonoBehaviour {
    public static Refs instance;

    public Material lightMaterial;
    public Material shadowMaterial;

    void Awake() {
        instance = this;
    }
}
