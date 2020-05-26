using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LightBase : MonoBehaviour {
    protected static int lightCounter = 0;
    private static Dictionary<int, LightBase> allLights = new Dictionary<int, LightBase>();

    public abstract bool IsInDark(Vector2 point);

    public static bool IsInDarkAllLights(Vector2 point) {
        foreach (var light in LightBase.GetLights()) {
            if (!light.IsInDark(point)) {
                return false;
            }
        }
        return true;
    }

    public virtual void Awake() {
        allLights.Add(gameObject.GetInstanceID(), this);
    }
    
    public virtual void OnDestroy() {
        allLights.Remove(gameObject.GetInstanceID());
    }

    public static Dictionary<int, LightBase>.ValueCollection GetLights() {
        return allLights.Values;
    }
}
