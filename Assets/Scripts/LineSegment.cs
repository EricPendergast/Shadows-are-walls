using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

[System.Serializable]
public struct LineSegment : IEnumerable<Vector2> {
    public Vector2 p1;
    public Vector2 p2;
    public static readonly LineSegment zero = new LineSegment(Vector2.zero, Vector2.zero);

    public LineSegment(Vector2 p1, Vector2 p2) {
        this.p1 = p1;
        this.p2 = p2;
    }

    public Vector2 Intersect(LineSegment other) {
        LineSegmentLib.LineSegmentsIntersection(p1, p2, other.p1, other.p2, out var intersection);
        return intersection;
    }

    public LineSegment Intersect(Triangle triangle) {
        foreach (var seg in Split(triangle.GetSides())) {
            if (triangle.Contains(seg.Midpoint())) {
                return seg;
            }
        }

        return zero;
    }

    public List<LineSegment> Split(LineSegment other) {
        if (LineSegmentLib.LineSegmentsIntersection(p1, p2, other.p1, other.p2, out var intersection)) {
            return new List<LineSegment>{new LineSegment(p1, intersection), new LineSegment(intersection, p2)};
        } else {
            return new List<LineSegment>{this};
        }
    }

    public List<LineSegment> Split(List<LineSegment> splits) {
        var ret = new List<LineSegment>{this};
        foreach(var split in splits) {
            ret = Math.SplitAll(ret, split);
        }
        return ret;
    }

    public Vector2 Midpoint() {
        return (p1 + p2)/2;
    }

    public IEnumerator<Vector2> GetEnumerator() {
        yield return p1;
        yield return p2;
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    public bool isValid() {
        return p1 != p2;
    }

    public bool OnRightSide(Vector2 point) {
        return Math.OnRightSide(point, this);
    }
}
