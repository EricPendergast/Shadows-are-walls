using UnityEngine;
using UnityEngine.Assertions;

public class ShadowEdge : DividesLight {
    [SerializeField]
    private Opaque caster;
    [SerializeField]
    private LightBase lightSource;
    bool first = true;

    public void Init(Opaque caster, LightBase lightSource) {
        this.caster = caster;
        this.lightSource = lightSource;
        Debug.Log("Initialized!!");
    }

    public override LineSegment GetDivider() {
        return target;
    }

    protected virtual void FixedUpdate() {
        if (first) {
            Debug.Log("First fixedupdate!!");
            first = false;
        }
        UpdateColliders();
        AddSimpleForces();
    }
}
