using System.Collections.Generic;
using UnityEngine;

public class Math {
    public static float Cross(Vector2 v1, Vector2 v2) {
        return v1.x*v2.y - v2.x*v1.y;
    }

    public static bool OnRightSide(Vector2 point, LineSegment line) {
        return Cross(line.p2 - line.p1, point - line.p1) > 0;
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
}
