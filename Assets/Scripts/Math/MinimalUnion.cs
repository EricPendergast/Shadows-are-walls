using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public static class MinimalUnion<T> {
    public struct CupWrapper {
        private static ListPool<Cup> pool = new ListPool<Cup>();
        public Cup v;
        public T obj;
        public CupWrapper(Cup c, T obj) { 
            v = c;
            this.obj = obj;
        }
        public static implicit operator Cup(CupWrapper c) => c.v;
        public List<CupWrapper> Subtract(Cup other, in List<CupWrapper> output) {
            output.Clear();
            using (var tmp = pool.TakeTemporary()) {
                foreach (Cup c in v.Subtract(other, tmp.val)) {
                    output.Add(new CupWrapper(c, obj));
                }
            }
            return output;
        }
    }

    // TODO: This is a garbage collection nightmare
    // Subtracts each cup in 'cups' from 'takeFrom', one by one. The reason
    // this looks complicated is that cup subtraction can yield 0, 1, or 2
    // cups.

    private static ListPool<CupWrapper> subtractListPool = new ListPool<CupWrapper>();
    public static Queue<CupWrapper> Subtract(CupWrapper takeFrom, IEnumerable<CupWrapper> cupsIn, in Queue<CupWrapper> cupsOut) {
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
    }

    private static List<CupWrapper> allShadows = new List<CupWrapper>();
    private static List<CupWrapper> minimalUnion = new List<CupWrapper>();
    private static Queue<CupWrapper> toTrim = new Queue<CupWrapper>();
    // Assumes that every Cup passed in has the same convergence point
    // Input does not need to be sorted
    // Output may not be sorted
    public static void Calculate(ref List<System.Tuple<LineSegment, T>> shadowsIn, Vector2 convergencePoint, System.Func<Vector2, float> metric) {
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

        allShadows.Sort((s1, s2) => metric(s1.v.p1).CompareTo(metric(s2.v.p1)));

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

        foreach (CupWrapper cup in minimalUnion) {
            shadowsIn.Add(System.Tuple.Create(cup.v.Base(), cup.obj));
        }
    }

}


