using UnityEngine; 
using UnityEngine.Assertions; 
using System.Collections.Generic;

public interface Convex {
    bool Contains(in Vector2 point, float epsilon);
    // Any convex shape will intersect a line segment at most twice
    // Output requirement: if i1 is null, i2 must also be null
    void IntersectEdge(in LineSegment seg, out Vector2? i1, out Vector2? i2, float epsilon);
}
// Represents a "cup" shape. Specifically, one line segment with a
// ray attached to each of its endpoints, where the rays always lie
// on the same side of the line segment, and diverge from each other.
//
// The way the shape is represented is by the two endpoints of the
// line segment, and the point where the two rays would intersect if
// they were full lines.
//
// This shape is useful for representing the shape of the shadow of a
// line segment.
[System.Serializable]
public readonly struct Cup : Convex {
    readonly public Vector2 p1;
    readonly public Vector2 p2;
    readonly public Vector2 convergencePoint;
    public static ListPool<LineSegment> pool = new ListPool<LineSegment>();

    public Cup(in LineSegment seg, in Vector2 conv) {
        p1 = seg.p1;
        p2 = seg.p2;
        convergencePoint = conv;
    }
    public Cup(in Vector2 p1, in Vector2 p2, in Vector2 conv) {
        this.p1 = p1;
        this.p2 = p2;
        convergencePoint = conv;
    }

    public LineSegment LeftRay() {
        return new LineSegment(p1, Math.Extend(convergencePoint, p1, 1000));
    }
    public LineSegment RightRay() {
        return new LineSegment(p2, Math.Extend(convergencePoint, p2, 1000));
    }

    public LineSegment Base() {
        return new LineSegment(p1, p2);
    }

    public IEnumerable<LineSegment> GetSides() {
        yield return LeftRay();
        yield return RightRay();
        yield return Base();
    }

    public bool Contains(in Vector2 point, float epsilon) {
        var p1p2 = new LineSegment(p1, p2);

        var epsilonSq = epsilon*epsilon;
        var halfEpsilonSq = (epsilon/2)*(epsilon/2);

        float dist1 = new LineSegment(p1, convergencePoint).SignedDistanceSq(point);
        float dist2 = p1p2.SignedDistanceSq(point);
        float dist3 = new LineSegment(convergencePoint, p2).SignedDistanceSq(point);

        bool fullyInside = (dist1 > halfEpsilonSq && dist2 > halfEpsilonSq && dist3 > halfEpsilonSq) ||
                           (dist1 < -halfEpsilonSq && dist2 < -halfEpsilonSq && dist3 < -halfEpsilonSq);

        if (fullyInside && p1p2.OnRightSideOfLine(point) != p1p2.OnRightSideOfLine(convergencePoint)) {
            return true;
        }

        bool fullyOutside = !(dist1 > -epsilonSq && dist2 > -epsilonSq && dist3 > -epsilonSq) &&
                            !(dist1 < epsilonSq && dist2 < epsilonSq && dist3 < epsilonSq);
                            
        if (fullyOutside) {
            return false;
        }

        return OnEdge(point, epsilon);
    }

    public bool OnEdge(Vector2 point, float epsilon) {
        return 
            (point - LineSegmentLib.ClosestPointOnRay(p1, p1 - convergencePoint, point)).sqrMagnitude < epsilon*epsilon ||
            (point - LineSegmentLib.ClosestPointOnRay(p2, p2 - convergencePoint, point)).sqrMagnitude < epsilon*epsilon ||
            (point - LineSegmentLib.ClosestPointOnLineSeg(p1, p2, point)).sqrMagnitude < epsilon*epsilon;
    }

    // epsilon -- if two intersections are within epsilon of each other, they
    // are considered one intersecion.
    public void IntersectEdge(in LineSegment seg, out Vector2? i1_o, out Vector2? i2_o, float epsilon) {
        // TODO: I'm sure there are floating point issues with using rays.
        // There should probably be a specialized ray intersect function.
        Vector2? i1 = seg.IntersectRay(p1, 2*p1 - convergencePoint, epsilon/3);
        Vector2? i2 = seg.IntersectRay(p2, 2*p2 - convergencePoint, epsilon/3);
        Vector2? i3 = seg.Intersect(Base(), epsilon/3);

        Util.RemoveDuplicates(ref i1, ref i2, ref i3, epsilon);
        Util.SlideDown(ref i1, ref i2, ref i3);

        Assert.IsTrue(i3 == null);
        i1_o = i1;
        i2_o = i2;
    }

    // This isn't actually a true subtraction, it is more like
    // this.Base() - other
    public List<Cup> Subtract(in Cup other, in List<Cup> output) {
        // TODO: WHY ISN'T OUTPUT CLEARED HERE
        Assert.AreEqual(convergencePoint, other.convergencePoint);
        using (var tmp = pool.TakeTemporary()) {
            foreach (var seg in Base().Subtract(other, tmp.val)) {
                output.Add(new Cup(seg, convergencePoint));
            }
        }
        return output;
    }

    public bool Subtract(in Cup other, out Cup? part1, out Cup? part2, float epsilon) {
        bool ret = Base().Subtract(other, out var seg1, out var seg2, epsilon);

        part1 = null;
        part2 = null;

        if (seg1 is LineSegment s1) {
            part1 = new Cup(s1, convergencePoint);
        }
        if (seg2 is LineSegment s2) {
            part2 = new Cup(s2, convergencePoint);
        }
        return ret;
    }

    public Cup Swapped() {
        return new Cup(p2, p1, convergencePoint);
    }

    //public static List<Cup> GetDisjoint(Cup c1, Cup c2) {
    //    var ret = new List<Cup>();
    //    ret.AddRange(c1.Subtract(c2))
    //    ret.AddRange(c2.Subtract(c1));
    //    return ret;
    //}
}
