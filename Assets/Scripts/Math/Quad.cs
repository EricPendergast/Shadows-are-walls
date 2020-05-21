using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Quad {
    public Vector2 p1;
    public Vector2 p2;
    public Vector2 p3;
    public Vector2 p4;

    public Quad(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4) {
        this.p1 = p1;
        this.p2 = p2;
        this.p3 = p3;
        this.p4 = p4;
    }

    public void Draw(List<Vector3> verts, List<int> indices) {
        int start = verts.Count;
        verts.AddRange(new Vector3[]{p1, p2, p3, p4});
        indices.AddRange(new int[]{
            start+0, start+1, start+2,
            start+0, start+2, start+3});
    }

    public List<LineSegment> GetSides() {
        return new List<LineSegment>{
            new LineSegment(p1, p2),
            new LineSegment(p2, p3),
            new LineSegment(p3, p4),
            new LineSegment(p4, p1)
        };
    }

    // Assumes this quad is convex
    public bool Contains(Vector2 point) {
        bool? prev = null;
        foreach (var side in GetSides()) {
            if (prev == null) {
                prev = side.OnRightSide(point);
            } else if (prev != side.OnRightSide(point)) {
                return false;
            }
        }
        return true;
    }
}
