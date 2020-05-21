﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LightBase : MonoBehaviour {
    private static Dictionary<int, LightBase> allLights = new Dictionary<int, LightBase>();

    public abstract Quad? GetShadowShape(LineSegment seg);
    public abstract bool IsInDark(Vector2 point);

    public static bool IsInDarkAllLights(Vector2 point) {
        foreach (var light in LightBase.GetLights()) {
            if (!light.IsInDark(point)) {
                return false;
            }
        }
        return true;
    }

    void Awake() {
        allLights.Add(gameObject.GetInstanceID(), this);
    }
    
    void OnDestroy() {
        allLights.Remove(gameObject.GetInstanceID());
    }

    public static Dictionary<int, LightBase>.ValueCollection GetLights() {
        return allLights.Values;
    }
}
