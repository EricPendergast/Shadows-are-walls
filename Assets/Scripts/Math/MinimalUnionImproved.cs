using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

using AngleFunc = System.Func<UnityEngine.Vector2, float>;

public static class MinimalUnionImproved<T> {
    private static float epsilon = .0001f;
    private static float angleEpsilon = .001f;

    private readonly struct CupWrapper : System.IComparable<CupWrapper> {
        private static ListPool<Cup> pool = new ListPool<Cup>();
        readonly public Cup v;
        readonly public T obj;
        readonly public float p1Angle;
        readonly public float p2Angle;
        public Vector2 p1 {get => v.p1;}
        public Vector2 p2 {get => v.p2;}

        public CupWrapper(Cup c, T obj, AngleFunc angleFunc) { 
            v = c;
            this.obj = obj;
            p1Angle = angleFunc(v.p1);
            p2Angle = angleFunc(v.p2);
        }
        private CupWrapper(Cup v, T obj, float p1Angle, float p2Angle) {
            this.v = v;
            this.obj = obj;
            this.p1Angle = p1Angle;
            this.p2Angle = p2Angle;
        }

        public static implicit operator Cup(CupWrapper c) => c.v;

        public CupWrapper Swapped() {
            return new CupWrapper(v.Swapped(), obj, p2Angle, p1Angle);
        }

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
    }

    private class MinimalUnion {
        private SortedSet<CupWrapper> currentCups = new SortedSet<CupWrapper>();
        private SortedSet<CupWrapper> dataTmp = new SortedSet<CupWrapper>();
        // Contains cups which can't be added to currentCups yet. A cup gets
        // added to here if in the process of adding a cup to currentCups, it
        // splits a cup into two pieces. The second piece would go in here
        // because it now comes after the cup which we are adding, so we aren't
        // sure if it can be added.
        private SortedSet<CupWrapper> futureCups = new SortedSet<CupWrapper>();
        private SortedSet<CupWrapper> resultCups = new SortedSet<CupWrapper>();

        private void Swap(ref SortedSet<CupWrapper> a, ref SortedSet<CupWrapper> b) {
            var tmp = a;
            a = b;
            b = tmp;
        }

        public void Add(CupWrapper cupW, AngleFunc angleFunc) {
            AddFutureCupsComingBefore(cupW, angleFunc);
            AddInternal(cupW, angleFunc);
        }

        private void AddFutureCupsComingBefore(CupWrapper? cupW, AngleFunc angleFunc) {
            int loopLimiter = 1000;
            while (futureCups.Count > 0) {
                var min = futureCups.Min;
                if (cupW != null && min.p1Angle > cupW?.p1Angle) {
                    break;
                }

                futureCups.Remove(min);
                AddInternal(min, angleFunc);

                if (loopLimiter-- < 0) {
                    Debug.Assert(false, "Too many loops");
                    break;
                }
            }
        }
        
        // PRECONDITION: Every cup passed into this function must have a larger
        // p1Angle than the previous cup passed in.
        // TODO: The problem is that subtracting cupW from the rest of the
        // cups can yield a cup in currentCups which has its p1 coming after cupW's
        // p1.
        private void AddInternal(CupWrapper cupW, AngleFunc angleFunc) {
            Debug.Assert(CupComesAfterExistingCups(cupW, angleFunc));
            if (!CupComesAfterExistingCups(cupW, angleFunc)) {
                Debug.Log("HeRE");
            }

            MoveCurrentNonOverlappingToResults(cupW, angleFunc);

            if (SubtractCurrentFrom(cupW, angleFunc) is CupWrapper cup) {
                CupWrapper trimmed = new CupWrapper(cup, cupW.obj, angleFunc);
                if (trimmed.p1Angle > cupW.p1Angle) {
                    futureCups.Add(trimmed);
                } else {
                    SubtractFromCurrent(cup, angleFunc);
                    currentCups.Add(trimmed);
                }
            }
        }

        private void MoveCurrentNonOverlappingToResults(CupWrapper? cupW, AngleFunc angleFunc) {
            bool MoveToResultsIfNonOverlapping(CupWrapper currentCup) {
                if (cupW == null || currentCup.p2Angle < cupW?.p1Angle) {
                    resultCups.Add(currentCup);
                    return true;
                }
                return false;
            }

            currentCups.RemoveWhere(MoveToResultsIfNonOverlapping);
        }

        private bool CupComesAfterExistingCups(in CupWrapper cup, AngleFunc angleFunc) {
            if (currentCups.Count > 0) {
                return Math.ApproxGeq(cup.p1Angle, currentCups.Max.p1Angle, angleEpsilon);
            } else if (resultCups.Count > 0) {
                return Math.ApproxGeq(cup.p1Angle, resultCups.Max.p1Angle, angleEpsilon);
            } else {
                return true;
            }
        }

        private CupWrapper? SubtractCurrentFrom(CupWrapper cupW, AngleFunc angleFunc) {
            Cup cup = cupW;
            foreach (CupWrapper dataCup in currentCups) {
                cup.Subtract(dataCup, out Cup? part1, out Cup? part2, epsilon);
                //Debug.Assert(part2 == null, "When first adding a cup, it should not be split into more than one piece.");
                if (part2 is Cup c2) {
                    futureCups.Add(new CupWrapper(c2, cupW.obj, angleFunc));
                }
                if (part1 is Cup c) {
                    cup = c;
                } else {
                    return null;
                }
            }
            return new CupWrapper(cup, cupW.obj, angleFunc);
        }

        private void SubtractFromCurrent(CupWrapper cup, AngleFunc angleFunc) {
            dataTmp.Clear();
            foreach (var cupW in currentCups) {
                cupW.v.Subtract(cup, out Cup? before, out Cup? after, epsilon);

                if (before != null && before?.p1 != cupW.p1) {
                    Debug.Assert(after == null);
                    after = before;
                    before = null;
                }

                if (before != null) {
                    var cup1 = new CupWrapper((Cup)before, cupW.obj, angleFunc);

                    resultCups.Add(cup1);
                }

                if (after != null) {
                    var cup2 = new CupWrapper((Cup)after, cupW.obj, angleFunc);

                    Debug.Assert(Math.ApproxGeq(cup2.p1Angle, cup.p2Angle, .001f));
                    if (!(Math.ApproxGeq(cup2.p1Angle, cup.p2Angle, .001f))) {
                        Debug.Log("HERE");
                    }

                    futureCups.Add(cup2);
                }
            }
            Swap(ref dataTmp, ref currentCups);
        }

        public IEnumerable<CupWrapper> GetFinalResults(AngleFunc angleFunc) {
            int loopLimiter = 100;
            while (futureCups.Count > 0) {
                if (loopLimiter-- < 0) {
                    Debug.Assert(false, "Too many loops");
                    break;
                }
                AddFutureCupsComingBefore(null, angleFunc);
            }
            MoveCurrentNonOverlappingToResults(null, angleFunc);
            return resultCups;
        }

        public void Reset() {
            currentCups.Clear();
            dataTmp.Clear();
            resultCups.Clear();
            futureCups.Clear();
        }
    }


    private static List<CupWrapper> allShadows = new List<CupWrapper>();
    private static MinimalUnion minimalUnion = new MinimalUnion();

    public static void SortedMinimalUnion(
            ref List<System.Tuple<LineSegment, T>> shadowsIn,
            Vector2 convergencePoint,
            AngleFunc angleFunc) {

        minimalUnion.Reset();
        allShadows.Clear();

        foreach (var segmentTuple in shadowsIn) {
            allShadows.Add(
                new CupWrapper(
                    new Cup(segmentTuple.Item1, convergencePoint),
                    segmentTuple.Item2,
                    angleFunc
                )
            );
        }

        SortCupWrappers(ref allShadows, angleFunc);

        foreach (var cupW in allShadows) {
            minimalUnion.Add(cupW, angleFunc);
        }

        UnwrapInto(
            minimalUnion.GetFinalResults(angleFunc),
            ref shadowsIn
        );

        //shadowsIn.Sort((s1, s2) => angleFunc(s1.Item1.Midpoint()).CompareTo(angleFunc(s2.Item1.Midpoint())));
    }

    private static void UnwrapInto(
            IEnumerable<CupWrapper> toUnwrap,
            ref List<System.Tuple<LineSegment, T>> unwrapInto) {

        unwrapInto.Clear();
        foreach (CupWrapper cup in toUnwrap) {
            unwrapInto.Add(System.Tuple.Create(cup.v.Base(), cup.obj));
        }
    }

    private static void SortCupWrappers(
            ref List<CupWrapper> cupWrappers,
            AngleFunc angleFunc
            ) {

        for (int i = 0; i < cupWrappers.Count; i++) {
            var cupWrapper = cupWrappers[i];
            if (angleFunc(cupWrapper.v.p1) > angleFunc(cupWrapper.v.p2)) {
                cupWrappers[i] = cupWrapper.Swapped();
            }
        }

        cupWrappers.Sort((s1, s2) => angleFunc(s1.v.p1).CompareTo(angleFunc(s2.v.p1)));
    }
}
