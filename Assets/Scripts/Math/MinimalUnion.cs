using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;

public static class MinimalUnion<T> {

    private static System.Func<Vector2, float> metric;
    
    private class CompareByMidpoint : Comparer<CupWrapper> {
        public override int Compare(CupWrapper a, CupWrapper b) {
            return ((a.p1Angle + a.p2Angle)/2).CompareTo((b.p1Angle + b.p2Angle)/2);
        }
    }

    private class CompareByP1 : Comparer<CupWrapper> {
        public override int Compare(CupWrapper a, CupWrapper b) {
            return a.p1Angle.CompareTo(b.p1Angle);
        }
    }

    public readonly struct CupWrapper {
        private static ListPool<Cup> pool = new ListPool<Cup>();
        public readonly Cup v;
        public readonly T obj;
        public readonly float p1Angle;
        public readonly float p2Angle;

        public CupWrapper(Cup c, T obj) { 
            v = c;
            this.obj = obj;
            var p1Angle = metric(c.p1);
            var p2Angle = metric(c.p2);

            if (p1Angle <= p2Angle) {
                this.p1Angle = p1Angle;
                this.p2Angle = p2Angle;
                v = new Cup(c.p1, c.p2, c.convergencePoint);
            } else {
                this.p1Angle = p2Angle;
                this.p2Angle = p1Angle;
                v = new Cup(c.p2, c.p1, c.convergencePoint);
            }

        }
        public static implicit operator Cup(in CupWrapper c) => c.v;
        public List<CupWrapper> Subtract(in CupWrapper other, in List<CupWrapper> output) {
            Profiler.BeginSample("CupWrapper.Subtract");

            output.Clear();
            if (!AngleOverlaps(other)) {
                output.Add(this);
                Profiler.EndSample();
                return output;
            }


            v.Subtract(other, out Cup? sub1, out Cup? sub2, .0001f);
            if (sub1 is Cup) {
                output.Add(new CupWrapper((Cup)sub1, obj));
            }
            if (sub2 is Cup) {
                output.Add(new CupWrapper((Cup)sub2, obj));
            }
            Profiler.EndSample();
            return output;
        }

        private bool AngleOverlaps(in CupWrapper other) {
            if (p1Angle > other.p1Angle) {
                return p1Angle < other.p2Angle;
            }
            if (p1Angle < other.p1Angle) {
                return p2Angle > other.p1Angle;
            }
            return p2Angle > other.p1Angle;
        }
    }

    // TODO: This is a garbage collection nightmare
    // Subtracts each cup in 'cups' from 'takeFrom', one by one. The reason
    // this looks complicated is that cup subtraction can yield 0, 1, or 2
    // cups.

    private static ListPool<CupWrapper> subtractListPool = new ListPool<CupWrapper>();
    public static Queue<CupWrapper> Subtract(CupWrapper takeFrom, IEnumerable<CupWrapper> cupsIn, in Queue<CupWrapper> cupsOut) {
        Profiler.BeginSample("MinimalUnion.Subtract");
        cupsOut.Clear();
        using (var tmp = subtractListPool.TakeTemporary()) {

            cupsOut.Enqueue(takeFrom);
            foreach (var cup in cupsIn) {
                int subtractedCount = cupsOut.Count;
                for (int i = 0; i < subtractedCount; i++) {
                    foreach (var cup2 in cupsOut.Dequeue().Subtract(cup, tmp.val)) {
                        cupsOut.Enqueue(cup2);
                    }
                }
            }
        }
        Profiler.EndSample();
        return cupsOut;
    }

    // TODO: This is a garbage collection nightmare
    // A simplified version of the main MinimalUnion function.
    // Assumes 'current' is already minimal
    // Assumes that every Cup passed in has the same convergence point
    private static QueuePool<CupWrapper> calcSubProbQueuePool = new QueuePool<CupWrapper>();
    //private static Queue<CupWrapper> newSegParts = new Queue<CupWrapper>();
    //private static Queue<CupWrapper> tmp = new Queue<CupWrapper>();
    public static void CalculateSubProblem(ref Queue<CupWrapper> current, CupWrapper newSeg) {
        Profiler.BeginSample("MinimalUnion.CalculateSubProblem");
        using (var newSegParts = calcSubProbQueuePool.TakeTemporary()) {
            using (var tmp = calcSubProbQueuePool.TakeTemporary()) {
                Subtract(newSeg, current, newSegParts.val);

                int currentCount = current.Count;
                for (int j = 0; j < currentCount; j++) {
                    var cupToCull = current.Dequeue();
                    foreach (var cup in Subtract(cupToCull, newSegParts.val, tmp.val)) {
                        current.Enqueue(cup);
                    }
                }

                foreach (var segPart in newSegParts.val) {
                    current.Enqueue(segPart);
                }
            }
        }
        Profiler.EndSample();
    }

    private static List<CupWrapper> allShadows = new List<CupWrapper>();
    private static List<CupWrapper> minimalUnion = new List<CupWrapper>();
    private static Queue<CupWrapper> toTrim = new Queue<CupWrapper>();
    // Assumes that every Cup passed in has the same convergence point
    // Input does not need to be sorted
    // Output is sorted by increasing angle.
    // Individual line segments are sorted by increasing angle as well. (i.e.
    // any line segment l has metric(l.p1) < metric(l.p2))
    public static void CalculateAndSort(ref List<System.Tuple<LineSegment, T>> shadowsIn, Vector2 convergencePoint, System.Func<Vector2, float> metric) {
        MinimalUnion<T>.metric = metric;

        allShadows.Clear();
        minimalUnion.Clear();
        toTrim.Clear();

        for (int i = 0; i < shadowsIn.Count; i++) {
            var seg = shadowsIn[i].Item1;
            var obj = shadowsIn[i].Item2;
            if (metric(seg.p1) > metric(seg.p2)) {
                seg = seg.Swapped();
            }
            allShadows.Add(new CupWrapper(new Cup(seg, convergencePoint), obj));
        }

        allShadows.Sort(new CompareByP1());

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
            CalculateSubProblem(ref toTrim, nextShadow);
        }

        foreach (var cup in toTrim) {
            minimalUnion.Add(cup);
        }

        shadowsIn.Clear();

        minimalUnion.Sort(new CompareByMidpoint());

        foreach (CupWrapper cup in minimalUnion) {
            shadowsIn.Add(System.Tuple.Create(cup.v.Base(), cup.obj));
        }
    }
}


