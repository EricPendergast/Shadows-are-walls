using System.Collections.Generic;
using UnityEngine;

public class Math {
    public static float Cross(Vector2 v1, Vector2 v2) {
        return v1.x*v2.y - v2.x*v1.y;
    }

    // epsilon specifies how far from the line is considered to be on the line
    public static bool OnRightSideOrOn(Vector2 point, LineSegment line, float epsilon) {
        return Cross(line.p2 - line.p1, point - line.p1)/(line.Length()) > -epsilon;
    }

    public static bool OnRightSide(Vector2 point, LineSegment line) {
        return Cross(line.p2 - line.p1, point - line.p1) > 0;
    }

    public static float SignedDistance(Vector2 point, LineSegment line) {
        return Cross(line.p2 - line.p1, point - line.p1)/(line.Length());
    }

    //public static IEnumerable<LineSegment> GetTriangleSides(Vector2 p1, Vector2 p2, Vector2 p3) {
    //        yield return new LineSegment(p1, p2);
    //        yield return new LineSegment(p2, p3);
    //        yield return new LineSegment(p3, p1);
    //}
    //public static bool IsInCone(Vector2 point, Vector2 left, Vector2 middle, Vector2 right) {
    //    var leftExtended = 1000000*((left-middle).normalized);
    //    var rightExtended = 1000000*((right-middle).normalized);
    //    return IsInTriangle(point, leftExtended, middle, rightExtended);
    //}
    //

    public static Vector3 Extend(Vector2 origin, Vector2 toExtend, float newDistance) {
        return origin + (toExtend - origin).normalized*newDistance;
    }

    public static Vector2 Rotate(Vector2 toRotate, float angle) {
        // This may not be the most efficient way to rotate a
        // Vector2, since Quaternions are for 3D rotations
        return Quaternion.Euler(0,0,angle)*toRotate;
    }

    public static float Mod(float x, float m) {
        return ((x % m) + m) % m;
    }

    public static int Mod(int x, int m) {
        return ((x % m) + m) % m;
    }

    /// Calculates the change in angle from angle1 to angle2, in the
    /// range [-180, 180]
    public static float AngleDifference(float angle1, float angle2 )
    {
        float diff = ( angle2 - angle1 + 180 ) % 360 - 180;
        return diff < -180 ? diff + 360 : diff;
    }

    /// Returns the counterclockwise difference from angle1 to angle2. This
    /// quantity is always in the range [0, 360]
    public static float CounterClockwiseAngleDifference(float angle1, float angle2) {
        float diffSigned = AngleDifference(angle1, angle2);
        return diffSigned > 0 ? diffSigned : 360 + diffSigned;
    }

    public static bool AnglesApproximatelyEqual(float a1, float a2) {
        return Mathf.Approximately(Math.Mod(a1, 360), Math.Mod(a2, 360)) ||
               Mathf.Approximately(Math.Mod(a1 + 180, 360), Math.Mod(a2 + 180, 360));
    }

    /// Returns whether 'angle' is in the region of the unit circle starting at
    /// 'lower', and going counterclockwise (increasing angle) to 'upper'.
    /// This will not wind around the circle multiple times. This means, for
    /// example, that LiesBetween(20, 0, 360+10) == false.
    public static bool LiesBetween(float angle, float lower, float upper) {
        return CounterClockwiseAngleDifference(lower, angle) <= CounterClockwiseAngleDifference(lower, upper);
    }

    /// Clamps 'angle' to the region of the unit circle starting at 'lower',
    /// and going counterclockwise (increasing angle) to 'upper'
    /// This will not wind around the circle multiple times. This means, for
    /// example, that LiesBetween(20, 0, 360+10) == false.
    public static float ClampAngle(float angle, float lower, float upper) {
        if (LiesBetween(angle, lower, upper)) {
            return angle;
        } else if (Mathf.Abs(Math.AngleDifference(angle, lower)) < Mathf.Abs(Math.AngleDifference(angle, upper))) {
            return lower;
        } else {
            return upper;
        }
    }

    public static bool ApproxGeq(float a, float b, float epsilon) {
        return a > (b-epsilon);
    }

    public static bool ApproxLeq(float a, float b, float epsilon) {
        return a < (b+epsilon);
    }

    public static bool ApproxEq(float a, float b, float epsilon) {
        return Mathf.Abs(a - b) < epsilon;
    }

    public static bool ApproxEq(Vector2 a, Vector2 b, float epsilon) {
        return (a - b).sqrMagnitude < epsilon * epsilon;
    }

    public static bool ApproxEq(Vector2? a, Vector2? b, float epsilon) {
        if (a == null && b == null) {
            return true;
        } else if (a != null && b != null) {
            return ApproxEq((Vector2)a, (Vector2)b, epsilon);
        } else {
            return false;
        }
    }

    // These are commented because I'm not sure what they ever would be used
    // for. And how they should be implemented is ambiguous. For instance, if a
    // == b, is b approximately greater than a, since its within an epsilon of
    // being greater than a? Or does b need more than epsilon greater than a?
    //public static bool ApproxGreater(float a, float b) {
    //    return !Mathf.Approximately(a, b) && a > b;
    //}
    //
    //public static bool ApproxLess(float a, float b) {
    //    return !Mathf.Approximately(a, b) && a < b;
    //}
}
