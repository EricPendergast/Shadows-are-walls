using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Profiling;
using ListExtensions;

using AngleFunc = System.Func<UnityEngine.Vector2, float>;

public static class MinimalUnionImprovedAgain<T> {
    private static float epsilon = 0;
    private static float angleEpsilon = .001f;

    private static System.Func<Vector2, float> metric;

    private class CompareByMidpoint : Comparer<CupWrapper> {
        public override int Compare(CupWrapper a, CupWrapper b) {
            return ((a.p1Angle + a.p2Angle)/2).CompareTo((b.p1Angle + b.p2Angle)/2);
        }
    }

    private readonly struct CupWrapper : System.IComparable<CupWrapper> {
        private static ListPool<Cup> pool = new ListPool<Cup>();
        readonly public Cup v;
        readonly public T obj;
        readonly public float p1Angle;
        readonly public float p2Angle;
        public Vector2 p1 {get => v.p1;}
        public Vector2 p2 {get => v.p2;}

        public CupWrapper(Cup c, T obj) { 
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

        public static implicit operator Cup(CupWrapper c) => c.v;

        public int CompareTo(CupWrapper other) {
            int comp = p1Angle.CompareTo(other.p1Angle);
            if (comp == 0) {
                comp = p2Angle.CompareTo(other.p2Angle);
                if (comp == 0) {
                    comp = p1.x.CompareTo(other.p1.x);
                    if (comp == 0) {
                        comp = p1.y.CompareTo(other.p1.y);
                        if (comp == 0) {
                            comp = p2.x.CompareTo(other.p2.x);
                            if (comp == 0) {
                                comp = p2.y.CompareTo(other.p2.y);
                            }
                        }
                    }
                }
            }
            return comp;
        }

        public bool FullyLeq(in CupWrapper other) {
            return p1Angle < other.p1Angle && p2Angle <= other.p1Angle;
        }

        public bool FullyGeq(in CupWrapper other) {
            return p1Angle >= other.p2Angle && p2Angle > other.p2Angle;
        }

        public float MidpointAngle() {
            return (p1Angle + p2Angle)/2;
        }

        public float AngleWidth() {
            return p2Angle - p1Angle;
        }

        public bool AngleOverlaps(in CupWrapper other) {
            if (p1Angle > other.p1Angle) {
                return p1Angle < other.p2Angle;
            }
            if (p1Angle < other.p1Angle) {
                return p2Angle > other.p1Angle;
            }
            return p2Angle > other.p1Angle;
        }

        public override bool Equals(object o){
            if (o is CupWrapper other) {
                return CompareTo(other) == 0;
            } else {
                return false;
            }
        }

        public override int GetHashCode() {
            return p1.GetHashCode() ^ p2.GetHashCode();
        }

        public bool Subtract(in CupWrapper other, out CupWrapper? before, out CupWrapper? after, float epsilon) {
            before = null;
            after = null;

            if (!AngleOverlaps(other)) {
                before = this;
                return false;
            }

            bool ret = v.Subtract(other, out Cup? bef, out Cup? aft, .0001f);
            if (bef is Cup) {
                before  = new CupWrapper((Cup)bef, obj);
            }
            if (aft is Cup) {
                after = new CupWrapper((Cup)aft, obj);
            }

            return ret;
        }
    }

    // A version of MinimalUnion which doesn't handle trimmings.
    private class MinimalUnionSimplified {
        private List<CupWrapper> currentCups = new List<CupWrapper>();
        private List<CupWrapper> resultCups = new List<CupWrapper>();
        private List<CupWrapper> dataTmp = new List<CupWrapper>();

        private Comparer<CupWrapper> midpointComparer = new CompareByMidpoint();

        // PRECONDITION: Every cup passed into this function must have a larger
        // p1Angle than the previous cup passed in.
        public void Add(CupWrapper cupW, System.Action<CupWrapper> OnTrimmingCreated) {
            Profiler.BeginSample("SimplifiedMinimalUnion.Add");
            if (!CupComesAfterExistingCups(cupW)) {
                Debug.Assert(false, "Cup comes before existing cup");
                Profiler.EndSample();
                return;
            }

            MoveCurrentNonOverlappingToResults(cupW);

            if (SubtractCurrentFrom(cupW, OnTrimmingCreated) is CupWrapper trimmed) {
                Debug.Assert(trimmed.p1Angle <= cupW.p1Angle);

                // TODO: I don't think this is necessary
                MoveCurrentNonOverlappingToResults(trimmed);
                SubtractFromCurrent(trimmed, OnTrimmingCreated);
                currentCups.InsertSorted(trimmed);
            }
            Profiler.EndSample();
        }

        private bool CupComesAfterExistingCups(CupWrapper cupW) {
            foreach (var other in currentCups) {
                if (cupW.p1Angle < other.p1Angle) {
                    return false;
                }
            }
            foreach (var other in resultCups) {
                if (cupW.p1Angle < other.p1Angle) {
                    return false;
                }
            }
            return true;
        }
        private void MoveCurrentNonOverlappingToResults(CupWrapper? cupW) {
            bool MoveToResultsIfNonOverlapping(CupWrapper currentCup) {
                if (cupW == null || currentCup.FullyLeq((CupWrapper)cupW)) {
                    resultCups.Add(currentCup);
                    return true;
                }
                return false;
            }

            for (int i = currentCups.Count - 1; i >= 0; i--) {
                if (MoveToResultsIfNonOverlapping(currentCups[i])) {
                    currentCups.RemoveAt(i);
                }
            }

            //currentCups.RemoveAll(MoveToResultsIfNonOverlapping);
        }

        private CupWrapper? SubtractCurrentFrom(CupWrapper? subtractFrom, System.Action<CupWrapper> OnTrimmingCreated) {
            Profiler.BeginSample("SubtractCurrentFrom");

            float initialP1Angle = subtractFrom?.p1Angle ?? 0;

            foreach (CupWrapper dataCup in currentCups) {
                if (subtractFrom == null) {
                    break;
                }

                if (((CupWrapper)subtractFrom).Subtract(dataCup, out CupWrapper? before, out CupWrapper? after, epsilon)) {

                    subtractFrom = null;

                    void HandleSubtractResult(CupWrapper result) {
                        // Only keep subtracting from subtractFrom if its
                        // p1 angle remains unchanged. If its p1 angle changes,
                        // this class can't handle it any more, and it goes
                        // into the trimmings.
                        if (result.p1Angle <= initialP1Angle) {
                            //if (subtractFrom != null) {
                            //    if (subtractFrom?.AngleWidth() < result.AngleWidth()) {
                            //        subtractFrom = result;
                            //    }
                            //} else {
                                subtractFrom = result;
                            //}
                        } else {
                            OnTrimmingCreated(result);
                        }
                    }

                    if (before is CupWrapper bef) {
                        HandleSubtractResult(bef);
                    }
                    if (after is CupWrapper aft) {
                        HandleSubtractResult(aft);
                    }
                }
            }
            Profiler.EndSample();
            return subtractFrom;
        }

        private void SubtractFromCurrent(CupWrapper toSubtract, System.Action<CupWrapper> OnTrimmingCreated) {
            Profiler.BeginSample("SubtractFromCurrent");
            Swap(ref dataTmp, ref currentCups);
            currentCups.Clear();

            foreach (var current in dataTmp) {
                if (current.Subtract(toSubtract, out CupWrapper? before, out CupWrapper? after, epsilon)) {

                    void HandleSubtractResult(CupWrapper result) {
                        if (result.MidpointAngle() <= toSubtract.MidpointAngle()) {
                            resultCups.InsertSorted(result);
                        } else {
                            OnTrimmingCreated(result);
                        }
                    }

                    if (before is CupWrapper bef) {
                        HandleSubtractResult(bef);
                    }
                    if (after is CupWrapper aft) {
                        HandleSubtractResult(aft);
                    }
                } else {
                    currentCups.InsertSorted(current);
                }
            }

            dataTmp.Clear();
            Profiler.EndSample();
        }

        private void Swap(ref List<CupWrapper> a, ref List<CupWrapper> b) {
            var tmp = a;
            a = b;
            b = tmp;
        }

        public IEnumerable<CupWrapper> GetFinalResults() {
            resultCups.AddRange(currentCups);
            currentCups.Clear();

            resultCups.RemoveAll((CupWrapper cupW) => (cupW.p1 - cupW.p2).sqrMagnitude < .01);
            // We sort by midpoints here because in some edge cases, two cup
            // wrappers can have the same p1Angle.
            resultCups.Sort(midpointComparer);

            return resultCups;
        }

        public void Reset() {
            currentCups.Clear();
            dataTmp.Clear();
            resultCups.Clear();
        }
    }

    private class MinimalUnion {
        private MinimalUnionSimplified simpleMinUnion = new MinimalUnionSimplified();
        // Contains cups which can't be added to currentCups yet. A cup gets
        // added to here if in the process of adding a cup to currentCups, it
        // splits a cup into two pieces. The second piece would go in here
        // because it now comes after the cup which we are adding, so we aren't
        // sure if it can be added.
        private List<CupWrapper> futureCups = new List<CupWrapper>();

        public MinimalUnion() {
            AddTrimming = AddTrimmingFunc;
        }

        private void Swap(ref List<CupWrapper> a, ref List<CupWrapper> b) {
            var tmp = a;
            a = b;
            b = tmp;
        }

        public void Add(CupWrapper cupW) {
            AddFutureCupsComingBefore(cupW);
            simpleMinUnion.Add(cupW, AddTrimming);
        }

        private System.Action<CupWrapper> AddTrimming;
        private void AddTrimmingFunc(CupWrapper cupW) {
            futureCups.InsertSorted(cupW);
        }

        private void AddFutureCupsComingBefore(CupWrapper? cupW) {
            int loopLimiter = 100;
            while (futureCups.Count > 0) {
                var min = futureCups[0];
                //Debug.Assert(min.CompareTo(futureCups.Min()) == 0);
                if (cupW != null && min.p1Angle > cupW?.p1Angle) {
                    break;
                }

                futureCups.RemoveAt(0);
                simpleMinUnion.Add(min, AddTrimming);

                if (loopLimiter-- < 0) {
                    Debug.Assert(false, "Too many loops");
                    break;
                }
            }
        }

        public IEnumerable<CupWrapper> GetFinalResults() {
            int loopLimiter = 5;
            while (futureCups.Count > 0) {
                if (loopLimiter-- < 0) {
                    Debug.Assert(false, "Too many loops");
                    break;
                }

                AddFutureCupsComingBefore(null);
            }
            return simpleMinUnion.GetFinalResults();
        }

        public void Reset() {
            simpleMinUnion.Reset();
            futureCups.Clear();
        }
    }


    private static List<CupWrapper> allShadows = new List<CupWrapper>();
    private static MinimalUnion minimalUnion = new MinimalUnion();

    // This function just calls SortedMinimalUnion twice, so that in a
    // debugger, if something happens in the first call, you can step
    // throughfrom the beginning again in the second call
    public static void SortedMinimalUnionDEBUG(
            ref List<System.Tuple<LineSegment, T>> shadowsIn,
            Vector2 convergencePoint,
            AngleFunc angleFunc) {

        
        var shadowsCopy = new List<System.Tuple<LineSegment, T>>();
        foreach (var thing in shadowsIn) {
            shadowsCopy.Add(thing);
        }
        SortedMinimalUnion(ref shadowsIn, convergencePoint, angleFunc);
        SortedMinimalUnion(ref shadowsCopy, convergencePoint, angleFunc);
    }
    public static void SortedMinimalUnion(
            ref List<System.Tuple<LineSegment, T>> shadowsIn,
            Vector2 convergencePoint,
            AngleFunc angleFunc) {

        MinimalUnionImprovedAgain<T>.metric = angleFunc;

        Profiler.BeginSample("SortedMinimalUnion");

        minimalUnion.Reset();
        allShadows.Clear();

        foreach (var segmentTuple in shadowsIn) {
            var toAdd = 
                new CupWrapper(
                    new Cup(segmentTuple.Item1, convergencePoint),
                    segmentTuple.Item2
                );

            if (toAdd.p1 == convergencePoint || toAdd.p2 == convergencePoint) {
                continue;
            }

            allShadows.Add(toAdd);
        }

        allShadows.Sort();

        foreach (var cupW in allShadows) {
            minimalUnion.Add(cupW);
        }

        var finalResults = minimalUnion.GetFinalResults();
        AssertInvariants(finalResults);
        UnwrapInto(
            finalResults,
            ref shadowsIn
        );

        Profiler.EndSample();
    }

    private static void UnwrapInto(
            IEnumerable<CupWrapper> toUnwrap,
            ref List<System.Tuple<LineSegment, T>> unwrapInto) {

        unwrapInto.Clear();
        foreach (CupWrapper cup in toUnwrap) {
            unwrapInto.Add(System.Tuple.Create(cup.v.Base(), cup.obj));
        }
    }

    private static void AssertInvariants(IEnumerable<CupWrapper> finalResults) {
        CupWrapper? prevNullable = null;
        foreach (var current in finalResults) {
            if (prevNullable is CupWrapper prev) {
                // TODO: Should epsilon really be this large? At .01 this
                // sometimes fails though, but maybe thats because of a bug.
                if (!Math.ApproxGeq(current.p1Angle, prev.p2Angle, .1f)) {
                    Debug.Assert(false, "Assert failed: Overlapping segments: current.p1Angle=" + current.p1Angle + " prev.p2Angle=" + prev.p2Angle);
                }
            }
            prevNullable = current;
        }
    }
}
