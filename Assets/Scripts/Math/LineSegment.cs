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

    public Vector2? Intersect(LineSegment other) {
        if (LineSegmentLib.LineSegmentsIntersection(p1, p2, other.p1, other.p2, out var intersection)) {
            return intersection;
        }
        return null;
    }

    public LineSegment? Intersect(Triangle triangle) {
        foreach (var seg in Split(triangle.GetSides())) {
            if (triangle.Contains(seg.Midpoint())) {
                return seg;
            }
        }

        return null;
    }

    public LineSegment? Intersect(Cup cup) {
        foreach (var seg in Split(cup.GetSides())) {
            if (cup.Contains(seg.Midpoint())) {
                return seg;
            }
        }
        return null;
    }

    public List<LineSegment> Subtract(Cup cup) {
        var ret = new List<LineSegment>();
        foreach (var seg in Split(cup.GetSides())) {
            if (!cup.Contains(seg.Midpoint())) {
                ret.Add(seg);
            }
        }
        return ret;
    }

    public List<LineSegment> Split(LineSegment other) {
        if (LineSegmentLib.LineSegmentsIntersection(p1, p2, other.p1, other.p2, out var intersection)) {
            return new List<LineSegment>{new LineSegment(p1, intersection), new LineSegment(intersection, p2)};
        } else {
            return new List<LineSegment>{this};
        }
    }

    public List<LineSegment> Split(IEnumerable<LineSegment> splits) {
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

    public float Length() {
        return (p2 - p1).magnitude;
    }

    public float SqrLength() {
        return (p2 - p1).sqrMagnitude;
    }

    public float Angle() {
        return Vector2.SignedAngle(Vector2.right, p2 - p1);
    }

    // Measures the angle between the vectors pointing from p1 to p2 in this
    // and other
    public float Angle(LineSegment other) {
        return Vector2.SignedAngle(p2 - p1, other.p2 - other.p1);
    }

    // The point around which the angle is measured is p1
    public float Angle(Vector2 point) {
        return Vector2.SignedAngle(p2 - p1, point - p1);
    }

    public bool IsInLineWith(Vector2 point) {
        float angle = Vector2.Angle(p2 - p1, point - p1);
        return angle < .00001 || angle > 180 - .00001;
    }

    // Does the ray from p1 to p2 point away from 'point'?
    public bool GoesAwayFrom(Vector2 point) {
        return (p1 - point).sqrMagnitude < (p2 - point).sqrMagnitude;
    }

    public Vector2 GetRightSide() {
        Vector2 dir = (p2-p1).normalized*.001f;
        return Midpoint() + new Vector2(dir.y, -dir.x);
    }

    public Vector2 GetLeftSide() {
        Vector2 dir = (p2-p1).normalized*.001f;
        return Midpoint() + new Vector2(-dir.y, dir.x);
    }

    public override string ToString() {
        return "LineSegment(" + p1 + ", " + p2 + ")";
    }

    // Swaps the endpoints of the line segment
    public void Swap() {
        var tmp = p1;
        p1 = p2;
        p2 = tmp;
    }

    public LineSegment Swapped() {
        return new LineSegment(p2, p1);
    }
}
