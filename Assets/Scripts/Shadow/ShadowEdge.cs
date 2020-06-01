using UnityEngine;
using UnityEngine.Assertions;

public class ShadowEdge : DividesLight {
    [SerializeField]
    private Opaque caster;
    [SerializeField]
    private LightBase lightSource;

    private bool initialized = false;
    public void Init(Opaque caster, LightBase lightSource) {
        Assert.IsFalse(initialized);
        initialized = true;
        this.caster = caster;
        this.lightSource = lightSource;
    }

    void Start() {
        Assert.IsTrue(initialized);
    }

    public override LineSegment GetDivider() {
        return target;
    }

    public override void DoFixedUpdate() {
        UpdateColliders();
        AddSimpleForces();
    }
}
