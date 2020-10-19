using UnityEngine;

public class GlassObject : LevelObject {

    protected override void Awake() {
        base.Awake();

        gameObject.layer = PhysicsHelper.glassLayer;
    }
}
