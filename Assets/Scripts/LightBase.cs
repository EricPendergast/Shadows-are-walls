using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LightBase : MonoBehaviour {
    public abstract Quad? GetShadowShape(LineSegment seg);
}
