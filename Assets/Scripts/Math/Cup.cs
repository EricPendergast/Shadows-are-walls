using UnityEngine; 
using UnityEngine.Assertions; 
using System.Collections.Generic;

public interface Convex {
    bool Contains(in Vector2 point);
    // Any convex shape will intersect a line segment at most twice
    // Output requirement: if i1 is null, i2 must also be null
    void IntersectEdge(in LineSegment seg, out Vector2? i1, out Vector2? i2);
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

    public bool Contains(in Vector2 point) {
        bool side1 = new LineSegment(p1, convergencePoint).OnRightSide(point);
        bool side2 = new LineSegment(p1, p2).OnRightSide(point);
        bool side3 = new LineSegment(convergencePoint, p2).OnRightSide(point);
        return side1 == side2 && side2 == side3;
    }

    public void IntersectEdge(in LineSegment seg, out Vector2? i1_o, out Vector2? i2_o) {
        Vector2? i1 = seg.Intersect(LeftRay());
        Vector2? i2 = seg.Intersect(RightRay());
        Vector2? i3 = seg.Intersect(Base());

        Util.RemoveDuplicates(ref i1, ref i2, ref i3);
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

    //public static List<Cup> GetDisjoint(Cup c1, Cup c2) {
    //    var ret = new List<Cup>();
    //    ret.AddRange(c1.Subtract(c2))
    //    ret.AddRange(c2.Subtract(c1));
    //    return ret;
    //}
}
