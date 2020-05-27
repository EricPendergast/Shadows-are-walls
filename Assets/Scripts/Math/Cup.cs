using UnityEngine; 
using UnityEngine.Assertions; 
using System.Collections.Generic;

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
public struct Cup {
    public Vector2 p1;
    public Vector2 p2;
    public Vector2 convergencePoint;
    public static ListPool<LineSegment> pool = new ListPool<LineSegment>();

    public Cup(LineSegment seg, Vector2 conv) {
        p1 = seg.p1;
        p2 = seg.p2;
        convergencePoint = conv;
    }
    public Cup(Vector2 p1, Vector2 p2, Vector2 conv) {
        this.p1 = p1;
        this.p2 = p2;
        convergencePoint = conv;
    }

    public LineSegment LeftRay() {
        return new LineSegment(p1, Math.Extend(convergencePoint, p1, 1000000));
    }
    public LineSegment RightRay() {
        return new LineSegment(p2, Math.Extend(convergencePoint, p2, 1000000));
    }

    public LineSegment Base() {
        return new LineSegment(p1, p2);
    }

    public IEnumerable<LineSegment> GetSides() {
        yield return LeftRay();
        yield return RightRay();
        yield return Base();
    }

    public bool Contains(Vector2 point) {
        bool side1 = LeftRay().Swapped().OnRightSide(point);
        bool side2 = Base().OnRightSide(point);
        bool side3 = RightRay().OnRightSide(point);
        return side1 == side2 && side2 == side3;
    }

    // This isn't actually a true subtraction, it is more like
    // this.Base() - other
    public List<Cup> Subtract(Cup other, in List<Cup> output) {
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
