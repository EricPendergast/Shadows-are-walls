using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

[System.Serializable]
public readonly struct LineSegment : IEnumerable<Vector2> {
    public readonly Vector2 p1;
    public readonly Vector2 p2;
    public static readonly LineSegment zero = new LineSegment(Vector2.zero, Vector2.zero);
    private static ListPool<LineSegment> LSPool = new ListPool<LineSegment>();
    private static ListPool<Vector2> V2Pool = new ListPool<Vector2>();

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
        using (var tmp = LSPool.TakeTemporary()) {
            foreach (var seg in Split(triangle.GetSides(), tmp.val)) {
                if (triangle.Contains(seg.Midpoint())) {
                    return seg;
                }
            }
        }

        return null;
    }

    public LineSegment? Intersect(Cup cup) {
        using (var tmp = LSPool.TakeTemporary()) {
            foreach (var seg in Split(cup.GetSides(), tmp.val)) {
                if (cup.Contains(seg.Midpoint())) {
                    return seg;
                }
            }
        }
        return null;
    }

    public List<LineSegment> Subtract(Cup cup, in List<LineSegment> output) {
        output.Clear();
        using (var tmp = LSPool.TakeTemporary()) {
            // TODO: We can do this without a temporary variable, decide if that would be useful
            foreach (var seg in Split(cup.GetSides(), tmp.val)) {
                if (!cup.Contains(seg.Midpoint())) {
                    output.Add(seg);
                }
            }
        }
        return output;
    }

    public List<LineSegment> Split(LineSegment other, in List<LineSegment> output) {
        output.Clear();
        if (LineSegmentLib.LineSegmentsIntersection(p1, p2, other.p1, other.p2, out var intersection)) {
            output.Add(new LineSegment(p1, intersection));
            output.Add(new LineSegment(intersection, p2));
        } else {
            output.Add(this);
        }
        return output;
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

    public LineSegment Swapped() {
        return new LineSegment(p2, p1);
    }

    public List<LineSegment> Split(IEnumerable<LineSegment> splits, in List<LineSegment> output) {
        output.Clear();

        using (var intersections = V2Pool.TakeTemporary()) {
            GetIntersections(splits, intersections.val);

            float totalLength = this.Length();
            intersections.val.Insert(0, this.p1);
            intersections.val.Add(this.p2);

            // Because of lambda capture on value types
            var p1 = this.p1;
            intersections.val.Sort((v1, v2) => (p1 - v1).sqrMagnitude.CompareTo((p1 - v2).sqrMagnitude));

            for (int i = 1; i < intersections.val.Count; i++) {
                output.Add(new LineSegment(intersections.val[i-1], intersections.val[i]));
            }
        }
        return output;
    }

    List<Vector2> GetIntersections(IEnumerable<LineSegment> segs, in List<Vector2> output) {
        output.Clear();
        foreach (var seg in segs) {
            if (this.Intersect(seg) is Vector2 intersec) {
                output.Add(intersec);
            }
        }
        return output;
    }
}
