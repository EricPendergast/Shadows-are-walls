using System.Collections.Generic;
using UnityEngine;

public class Math {
    public static float Cross(Vector2 v1, Vector2 v2) {
        return Vector3.Cross((Vector3)v1, (Vector3)v2).z;
    }

    public static bool OnRightSide(Vector2 point, LineSegment line) {
        return Cross(line.p2 - line.p1, point - line.p1) > 0;
    }

    public static List<LineSegment> GetTriangleSides(Vector2 p1, Vector2 p2, Vector2 p3) {
        return new List<LineSegment>{
            new LineSegment(p1, p2),
            new LineSegment(p2, p3),
            new LineSegment(p3, p1)};
    }

    public static bool IsInTriangle(Vector2 point, Vector2 e1, Vector2 e2, Vector2 e3) {
        var sides = GetTriangleSides(e1, e2, e3);

        bool side1 = OnRightSide(point, sides[0]);
        bool side2 = OnRightSide(point, sides[1]);
        bool side3 = OnRightSide(point, sides[2]);

        return side1 == side2 && side2 == side3;
    }

    public static bool IsInCone(Vector2 point, Vector2 left, Vector2 middle, Vector2 right) {
        var leftExtended = 1000000*((left-middle).normalized);
        var rightExtended = 1000000*((right-middle).normalized);
        return IsInTriangle(point, leftExtended, middle, rightExtended);
    }

    // TODO: This can be very optimized
    public static List<LineSegment> SplitAll(List<LineSegment> all, LineSegment split) {
        var ret = new List<LineSegment>();
        foreach (var seg in all) {
            ret.AddRange(seg.Split(split));
        }
        return ret;
    }

    public static Vector3 Extend(Vector2 origin, Vector2 toExtend, float newDistance) {
        return origin + (toExtend - origin).normalized*newDistance;
    }

    // TODO: This is a garbage collection nightmare
    public static IEnumerable<Cup> Subtract(Cup takeFrom, IEnumerable<Cup> cups) {
        var subtracted = new Queue<Cup>();
        subtracted.Enqueue(takeFrom);
        foreach (Cup cup in cups) {
            int subtractedCount = subtracted.Count;
            for (int i = 0; i < subtractedCount; i++) {
                foreach (Cup cup2 in subtracted.Dequeue().Subtract(cup)) {
                    subtracted.Enqueue(cup2);
                }
            }
        }
        return subtracted;
    }

    // TODO: This is a garbage collection nightmare
    // Assumes that every Cup passed in has the same convergence point
    public static void MinimalUnion(Queue<Cup> current, Cup newSeg) {
        var newSegParts = Subtract(newSeg, current);

        int currentCount = current.Count;
        for (int j = 0; j < currentCount; j++) {
            var cupToCull = current.Dequeue();
            foreach (var cup in Subtract(cupToCull, newSegParts)) {
                current.Enqueue(cup);
            }
        }

        foreach (var segPart in newSegParts) {
            current.Enqueue(segPart);
        }
    }
}
