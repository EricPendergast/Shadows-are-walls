using UnityEngine;

public class ShadowEdge : DividesLight {
    [SerializeField]
    private Opaque caster;
    public void Init(LightBase lightSource, Side illuminatedSide, Opaque caster) {
        base.Init(lightSource, illuminatedSide);
        this.caster = caster;
    }

    public override LineSegment GetDivider() {
        return target;
    }

    public override void DoFixedUpdate() {
        UpdateColliders();
        AddSimpleForces();
    }
}
