using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[System.Serializable]
public readonly struct Triangle : Convex {
    readonly public Vector2 p1;
    readonly public Vector2 p2;
    readonly public Vector2 p3;

    readonly public LineSegment side0;
    readonly public LineSegment side1;
    readonly public LineSegment side2;

    public Triangle(Vector2 p1, Vector2 p2, Vector2 p3) {
        side0 = new LineSegment(p1, p2);
        side1 = new LineSegment(p2, p3);
        side2 = new LineSegment(p3, p1);
        this.p1 = p1;
        this.p2 = p2;
        this.p3 = p3;
    }

    public IEnumerable<LineSegment> GetSides() {
        yield return side0;
        yield return side1;
        yield return side2;
    }

    public bool Contains(in Vector2 point, float epsilon=.0001f) {

        float dist1 = side0.SignedDistance(point);
        float dist2 = side1.SignedDistance(point);
        float dist3 = side2.SignedDistance(point);

        return 
            (dist1 > -epsilon && dist2 > -epsilon && dist3 > -epsilon) ||
            (dist1 < epsilon && dist2 < epsilon && dist3 < epsilon);
    }

    public List<Vector2> AsList() {
        return new List<Vector2>{p1, p2, p3};
    }

    public void IntersectEdge(in LineSegment seg, out Vector2? i1_o, out Vector2? i2_o, float epsilon) {
        Vector2? i1 = seg.Intersect(side0, epsilon/3);
        Vector2? i2 = seg.Intersect(side1, epsilon/3);
        Vector2? i3 = seg.Intersect(side2, epsilon/3);

        Util.RemoveDuplicates(ref i1, ref i2, ref i3, epsilon);
        Util.SlideDown(ref i1, ref i2, ref i3);

        Assert.IsTrue(i3 == null);
        i1_o = i1;
        i2_o = i2;
    }

    public override string ToString() {
        return "Triangle(" + p1 + ", " + p2 + ", " + p3 + ")";
    }
}
