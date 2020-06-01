using UnityEngine;
using UnityEngine.Assertions;

public class LightEdge : DividesLight {
    [SerializeField]
    private LightBase lightSource;
    [SerializeField]
    private bool DEBUG = false;

    public void Init(LightBase lightSource) {
        Assert.IsFalse(initialized);
        initialized = true;
        this.lightSource = lightSource;
    }

    private bool initialized = false;
    void Start() {
        Assert.IsTrue(initialized);
    }

    public void SetTargetLength(float newLength) {
        target = target.WithLength(newLength);
    }

    public override LineSegment GetDivider() {
        return GetActual();
    }

    // TODO: I wonder if the order this executes is important, relative to ShadowEdge
    public override void DoFixedUpdate() {
        if (DEBUG) {
            Debug.Log("Break point");
        }
        UpdateColliders();
        AddSimpleForces();
    }
}
