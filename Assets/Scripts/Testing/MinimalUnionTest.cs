using System.Collections.Generic;
using UnityEngine;

using SegTuple = System.Tuple<LineSegment, string>;
public class MinimalUnionTest : MonoBehaviour {
    [System.Serializable]
    public class SegTupleClass {
        public Vector2 p1;
        public Vector2 p2;
        public string Item2;

        public SegTupleClass(LineSegment item1, string item2) {
            p1 = item1.p1;
            p2 = item1.p2;
            Item2 = item2;
        }
        public SegTuple ToTuple() {
            return System.Tuple.Create(new LineSegment(p1, p2), Item2);
        }
    }
    [SerializeField]
    public List<SegTupleClass> segs;

    public Vector2 lightPosition;
    public Vector2 metricStart;

    public float drawRadius = .25f;
    public bool drawCalculated = true;

    void Update() {
        if (!Input.GetMouseButton(0)) {
            return;
        }
        if (MouseMove(ref lightPosition)) {
            return;
        }
        for (int i = 0; i < segs.Count; i++) {
            var seg = segs[i];
            if (MouseMove(ref seg)) {
                segs[i] = seg;
                break;
            }
        }
    }

    bool MouseMove(ref SegTupleClass seg) {
        if (MouseMove(ref seg.p1)) {
            return true;
        } else if (MouseMove(ref seg.p2)) {
            return true;
        } else {
            return false;
        }
    }
    bool MouseMove(ref Vector2 point) {
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Input.GetMouseButton(0)) {
            if ((point - worldPosition).magnitude < drawRadius) {
                point = worldPosition;
                return true;
            }
        }
        return false;
    }

    void OnDrawGizmosSelected() {
        var segsCopy = new List<SegTuple>();
        foreach (var item in segs) {
            segsCopy.Add(item.ToTuple());
        }

        MinimalUnionImproved<string>.SortedMinimalUnion(
            ref segsCopy,
            lightPosition,
            (Vector2 vec) => 
                Vector2.Angle(metricStart, vec - lightPosition)
        );


        Gizmos.color = Color.white;
        foreach (var seg in segs) {
            Gizmos.DrawSphere((Vector3)seg.p1, drawRadius*1.5f);
            Gizmos.DrawSphere((Vector3)seg.p2, drawRadius*1.5f);
            Gizmos.DrawLine(seg.p1, seg.p2);
        }

        Gizmos.color = Color.red;
        foreach (SegTuple seg in segsCopy) {
            Gizmos.DrawSphere(seg.Item1.p1, drawRadius);
            Gizmos.DrawSphere(seg.Item1.p2, drawRadius);
            Gizmos.DrawLine(seg.Item1.p1, seg.Item1.p2);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(lightPosition, drawRadius * 2);

        Gizmos.DrawLine(lightPosition, lightPosition + metricStart.normalized*drawRadius*4);
    }
}
