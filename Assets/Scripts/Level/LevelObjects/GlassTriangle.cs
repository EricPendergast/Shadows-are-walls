using UnityEngine;

public class GlassTriangle : SolidTriangle {
    protected override void Awake() {
        base.Awake();
        gameObject.layer= PhysicsHelper.glassLayer;
    }
}
