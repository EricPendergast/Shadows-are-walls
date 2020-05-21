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

    public List<LineSegment> GetSides() {
        return Math.GetTriangleSides(p1, p2, p3);
    }

    public bool Contains(Vector2 point) {
        return Math.IsInTriangle(point, p1, p2, p3);
    }

    public List<Vector2> AsList() {
        return new List<Vector2>{p1, p2, p3};
    }
}
