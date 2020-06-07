using UnityEngine;

public class ShadowEdge : DividesLight {
    [SerializeField]
    private Opaque caster;
    private bool collidersEnabled;

    protected override void Awake() {
        base.Awake();
        collidersEnabled = true;
    }
    //public override void Init(LightBase lightSource, Side illuminatedSide) {
    //  Debug.Assert(false);
    //}

    public void Init(LightBase lightSource, Side illuminatedSide, Opaque caster) {
        base.Init(lightSource, illuminatedSide);
        this.caster = caster;
    }

    public override LineSegment GetDivider() {
        return target;
    }

    public override void DoFixedUpdate() {
        if (collidersEnabled) {
            UpdateColliders();
        }
        AddSimpleForces();
    }

    public void DisableColliders() {
        collidersEnabled = false;
        foreach (var collider in gameObject.GetComponents<Collider2D>()) {
            collider.enabled = false;
        }
    }
}
