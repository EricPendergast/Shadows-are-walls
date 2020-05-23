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

    public static List<LineSegment> MinimalUnion(IEnumerable<LineSegment> shadowsIn, Vector2 convergencePoint, System.Func<Vector2, float> metric) {
        var allShadows = new List<Cup>();

        System.Action<LineSegment> addShadow = (LineSegment s) => {
            if (metric(s.p1) > metric(s.p2)) {
                s.Swap();
            }
            allShadows.Add(new Cup(s, convergencePoint));
        };

        foreach (var shadow in shadowsIn) {
            addShadow(shadow);
        }

        allShadows.Sort((s1, s2) => metric(s1.p1).CompareTo(metric(s2.p1)));

        var minimalUnion = new List<LineSegment>();

        var toTrim = new Queue<Cup>();
        toTrim.Enqueue(allShadows[0]);
        
        for (int i = 1; i < allShadows.Count; i++) {
            var nextShadow = allShadows[i];
            int toTrimCount = toTrim.Count;
            for (int j = 0; j < toTrimCount; j++) {
                Cup c = toTrim.Dequeue();
                if (metric(c.p2) < metric(nextShadow.p1)) {
                    minimalUnion.Add(c.Base());
                } else {
                    toTrim.Enqueue(c);
                }
            }
            Math.MinimalUnion(toTrim, nextShadow);
        }

        foreach (Cup c in toTrim) {
            minimalUnion.Add(c.Base());
        }

        return minimalUnion;
    }
}
