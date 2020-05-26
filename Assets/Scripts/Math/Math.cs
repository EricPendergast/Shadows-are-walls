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


    public struct CupWrapper<T> {
        public Cup v;
        public T obj;
        public CupWrapper(Cup c, T obj) { 
            v = c;
            this.obj = obj;
        }
        public static implicit operator Cup(CupWrapper<T> c) => c.v;
        public List<CupWrapper<T>> Subtract(Cup other) {
            var ret = new List<CupWrapper<T>>();
            foreach (Cup c in v.Subtract(other)) {
                ret.Add(new CupWrapper<T>(c, obj));
            }
            return ret;
        }
    }

    // TODO: This is a garbage collection nightmare
    // Subtracts each cup in 'cups' from 'takeFrom', one by one. The reason
    // this looks complicated is that cup subtraction can yield 0, 1, or 2
    // cups.
    public static IEnumerable<CupWrapper<T>> Subtract<T>(CupWrapper<T> takeFrom, IEnumerable<CupWrapper<T>> cups) {
        var subtracted = new Queue<CupWrapper<T>>();
        subtracted.Enqueue(takeFrom);
        foreach (var cup in cups) {
            int subtractedCount = subtracted.Count;
            for (int i = 0; i < subtractedCount; i++) {
                foreach (var cup2 in subtracted.Dequeue().Subtract(cup)) {
                    subtracted.Enqueue(cup2);
                }
            }
        }
        return subtracted;
    }

    // TODO: This is a garbage collection nightmare
    // A simplified version of the main MinimalUnion function.
    // Assumes 'current' is already minimal
    // Assumes that every Cup passed in has the same convergence point
    private static void MinimalUnion<T>(Queue<CupWrapper<T>> current, CupWrapper<T> newSeg) {
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

    // Assumes that every Cup passed in has the same convergence point
    // Input does not need to be sorted
    // Output may not be sorted
    public static void MinimalUnion<T>(ref List<System.Tuple<LineSegment, T>> shadowsIn, Vector2 convergencePoint, System.Func<Vector2, float> metric) {
        var allShadows = new List<CupWrapper<T>>();

        for (int i = 0; i < shadowsIn.Count; i++) {
            var seg = shadowsIn[i].Item1;
            var obj = shadowsIn[i].Item2;
            if (metric(seg.p1) > metric(seg.p2)) {
                seg.Swap();
            }
            allShadows.Add(new CupWrapper<T>(new Cup(seg, convergencePoint), obj));
        }

        allShadows.Sort((s1, s2) => metric(s1.v.p1).CompareTo(metric(s2.v.p1)));

        var minimalUnion = new List<CupWrapper<T>>();

        var toTrim = new Queue<CupWrapper<T>>();
        toTrim.Enqueue(allShadows[0]);
        
        for (int i = 1; i < allShadows.Count; i++) {
            var nextShadow = allShadows[i];
            int toTrimCount = toTrim.Count;
            for (int j = 0; j < toTrimCount; j++) {
                var cup = toTrim.Dequeue();
                if (metric(cup.v.p2) < metric(nextShadow.v.p1)) {
                    minimalUnion.Add(cup);
                } else {
                    toTrim.Enqueue(cup);
                }
            }
            Math.MinimalUnion(toTrim, nextShadow);
        }

        foreach (var cup in toTrim) {
            minimalUnion.Add(cup);
        }

        shadowsIn.Clear();

        foreach (CupWrapper<T> cup in minimalUnion) {
            shadowsIn.Add(System.Tuple.Create(cup.v.Base(), cup.obj));
        }
    }
}
