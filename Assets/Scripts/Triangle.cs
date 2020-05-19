using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Triangle {
    public Vector2 p1;
    public Vector2 p2;
    public Vector2 p3;

    public Triangle(Vector2 p1, Vector2 p2, Vector2 p3) {
        this.p1 = p1;
        this.p2 = p2;
        this.p3 = p3;
    }
}
