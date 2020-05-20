using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The idea is that each shadow has an edge collider, and unity's physics
// determines where those colliders intersect. Then, using those intersection
// points, we create the proper box colliders so that objects collide with the
// shadows.

// Actualy I'm not doing that. This will be a centralized class to calculate
// the shadow boundary intersections for all the lights
public class Shadows : MonoBehaviour {
    public static Shadows instance;

    private List<LightBase> lights = new List<LightBase>();
    private float lastUpdated = 0;

    void Start() {
        instance = this;
        lights.Clear();
        lights.AddRange(FindObjectsOfType<LightBase>());
    }

    void Calculate() {
        //if (lastUpdated == Time.fixedUnscaledTime
    }
}
