using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using UnityEngine.Assertions; 

[System.Serializable]
public readonly struct LineSegment : IEnumerable<Vector2> {
    public readonly Vector2 p1;
    public readonly Vector2 p2;
    public static readonly LineSegment zero = new LineSegment(Vector2.zero, Vector2.zero);
    private static ListPool<LineSegment> LSPool = new ListPool<LineSegment>();
    private static ListPool<Vector2> V2Pool = new ListPool<Vector2>();

    public LineSegment(in Vector2 p1, in Vector2 p2) {
        this.p1 = p1;
        this.p2 = p2;
    }

    public Vector2? Intersect(in LineSegment other) {
        if (LineSegmentLib.LineSegmentsIntersection(p1, p2, other.p1, other.p2, out var intersection)) {
            return intersection;
        }
        return null;
    }

    public LineSegment? Intersect(in Convex convex) {

        convex.IntersectEdge(this, out Vector2? i1, out Vector2? i2);
        
        // There are no intersections in this case
        if (i1 == null) {
            if (convex.Contains(Midpoint())) {
                return this;
            }
            return null;
        }
        // There is one intersection in this case
        if (i2 == null) {
            LineSegment split1 = new LineSegment(p1, (Vector2)i1);
            if (split1.p1 != split1.p2 && convex.Contains(split1.Midpoint())) {
                return split1;
            }
            LineSegment split2 = new LineSegment((Vector2)i1, p2);
            if (split2.p1 != split2.p2 && convex.Contains(split2.Midpoint())) {
                return split2;
            }
            return null;
        }

        // There are two intersections otherwise, so the intersection must be
        // between the two intersection points
        return new LineSegment((Vector2)i1, (Vector2)i2);
    }

    public void Subtract(in Convex convex, out LineSegment? seg1, out LineSegment? seg2) {
        seg1 = null;
        seg2 = null;
        if (Intersect(convex) is LineSegment intersection) {
            // Make it so intersection.p1 is closest to this.p1
            if ((p1 - intersection.p1).sqrMagnitude > (p1 - intersection.p2).sqrMagnitude) {
                intersection = intersection.Swapped();
            }
            var part1 = new LineSegment(p1, intersection.p1);
            var part2 = new LineSegment(intersection.p2, p2);

            if (part1.p1 != part1.p2 && !convex.Contains(part1.Midpoint())) {
                seg1 = part1;
            }
            if (part2.p1 != part2.p2 && !convex.Contains(part2.Midpoint())) {
                seg2 = part2;
            }
            if (seg1 == null) {
                seg1 = seg2;
                seg2 = null;
            }
            return;
        } else {
            if (!convex.Contains(Midpoint())) {
                seg1 = this;
            }
            return;
        }
    }
    public List<LineSegment> Subtract(in Convex convex, in List<LineSegment> output) {
        output.Clear();
        Subtract(convex, out var seg1, out var seg2);
        if (seg1 is LineSegment s1) {
            output.Add(s1);
        }
        if (seg2 is LineSegment s2) {
            output.Add(s2);
        }
        return output;
    }

    public List<LineSegment> Split(in LineSegment other, in List<LineSegment> output) {
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

    public bool OnRightSide(in Vector2 point) {
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
    public float Angle(in LineSegment other) {
        return Vector2.SignedAngle(p2 - p1, other.p2 - other.p1);
    }

    public float UnsignedAngle(in LineSegment other) {
        return Vector2.Angle(p2 - p1, other.p2 - other.p1);
    }

    // The point around which the angle is measured is p1
    public float Angle(in Vector2 point) {
        return Vector2.SignedAngle(p2 - p1, point - p1);
    }

    // Does the ray from p1 to p2 point away from 'point'?
    public bool GoesAwayFrom(in Vector2 point) {
        return (p1 - point).sqrMagnitude < (p2 - point).sqrMagnitude;
    }

    // Gets the point just to the right of the midpoint of this line segment,
    // as if you are standing at p1 and looking at p2
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

    public List<LineSegment> Split(in IEnumerable<LineSegment> splits, in List<LineSegment> output) {
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

    public static bool operator==(LineSegment lhs, LineSegment rhs) {
        return lhs.p1 == rhs.p1 && lhs.p2 == rhs.p2;
    }

    public static bool operator!=(LineSegment lhs, LineSegment rhs) {
        return !(lhs == rhs);
    }

    public static LineSegment operator-(LineSegment lhs, Vector2 rhs) {
        return new LineSegment(lhs.p1 - rhs, lhs.p2 - rhs);
    }
    public static LineSegment operator+(LineSegment lhs, Vector2 rhs) {
        return new LineSegment(lhs.p1 + rhs, lhs.p2 + rhs);
    }

    // Changes the length of the line segment, with p1 staying fixed
    public LineSegment WithLength(float newLength) {
        if (p1 == p2) {
            return new LineSegment(p1, p1);
        }
        return new LineSegment(p1, p1 + (p2 - p1).normalized * newLength);
    }

    public Vector2 PerpendicularComponent(Vector2 v) {
        return v - (Vector2)Vector3.Project(v, (p1 - p2));
    }

    public LineSegment Rotate(float angle) {
        return new LineSegment(p1, p1 + (Vector2)(Quaternion.Euler(0,0,angle)*(p2 - p1)));
    }

    // The distance from this line segment, extended to a complete line, to the
    // given point
    // TODO: This has not been verified to work yet
    public float Distance(Vector2 point) {
        var top = Mathf.Abs((p2.y-p1.y)*point.x - (p2.x-p1.x)*point.y + p2.x*p1.y - p2.y*p1.x);
        var bottom = Mathf.Sqrt(Mathf.Pow(p2.y - p1.y, 2) + Mathf.Pow(p2.x - p1.x, 2));
        return top/bottom;
    }

    // Gives the closest point on this line segment (extended to a full line)
    // to 'point'
    public Vector2 Closest(Vector2 point) {
        var lineDir = (p2 - p1).normalized;
        var v = point - p1;
        var d = Vector3.Dot(v, lineDir);
        return p1 + lineDir * d;
    }
}
