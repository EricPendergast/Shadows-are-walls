using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Triangle {
    private Vector2 _p1;
    public Vector2 p1 {
        get { return _p1; }
        set { _p1 = side0.p1 = side2.p2 = value; }
    }
    private Vector2 _p2;
    public Vector2 p2 {
        get { return _p2; }
        set { _p2 = side0.p2 = side1.p1 = value; }
    }
    private Vector2 _p3;
    public Vector2 p3 {
        get { return _p3; }
        set { _p3 = side1.p2 = side2.p1 = value; }
    }

    private LineSegment side0;
    private LineSegment side1;
    private LineSegment side2;

    public Triangle(Vector2 p1, Vector2 p2, Vector2 p3) {
        side0 = new LineSegment(p1, p2);
        side1 = new LineSegment(p2, p3);
        side2 = new LineSegment(p3, p1);
        this._p1 = p1;
        this._p2 = p2;
        this._p3 = p3;
    }

    public IEnumerable<LineSegment> GetSides() {
        yield return side0;
        yield return side1;
        yield return side2;
    }

    public bool Contains(Vector2 point) {
        bool s1 = Math.OnRightSide(point, side0);
        bool s2 = Math.OnRightSide(point, side1);
        bool s3 = Math.OnRightSide(point, side2);

        return s1 == s2 && s2 == s3;
    }

    public List<Vector2> AsList() {
        return new List<Vector2>{p1, p2, p3};
    }

    public override string ToString() {
        return "Triangle(" + p1 + ", " + p2 + ", " + p3 + ")";
    }
}
