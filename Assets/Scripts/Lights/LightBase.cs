using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LightBase : AllTracker<LightBase> {
    protected static int lightCounter = 0;

    public abstract bool IsInDark(Vector2 point);
    public abstract bool IsIlluminated(Vector2 point);
    public abstract Rigidbody2D GetEdgeMountPoint();

    public abstract void DoFixedUpdate();

    public static bool IsInDarkAllLights(Vector2 point) {
        foreach (var light in LightBase.GetLights()) {
            if (!light.IsInDark(point)) {
                return false;
            }
        }
        return true;
    }

    protected override void Awake() {
        base.Awake();
        gameObject.layer = PhysicsHelper.lightLayer;
    }
    
    public static IEnumerable<LightBase> GetLights() {
        return GetAll();
    }

    public abstract Vector2 GetTargetPosition();

    public abstract float GetAngularVelocity();
}
