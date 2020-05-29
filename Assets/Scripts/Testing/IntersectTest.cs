using UnityEngine;
using System.Collections.Generic;


public class IntersectTest : MonoBehaviour {
    public Vector2 seg_p1;
    public Vector2 seg_p2;
    public Vector2 cup_p1;
    public Vector2 cup_p2;
    public Vector2 cup_p3;

    //public Triangle triangle;

    bool cupSubtraction = true;
    //bool cupIntersection = true;
    //bool triangleIntersection = false;


    void OnDrawGizmosSelected() {
        LineSegment seg = new LineSegment(seg_p1, seg_p2);
        Cup cup = new Cup(cup_p1, cup_p2, cup_p3);
        Gizmos.color = Color.white;
        if (cupSubtraction) {
            foreach (var side in cup.GetSides()) {
                Gizmos.DrawLine(side.p1, side.p2);
            }
            Gizmos.DrawSphere(seg.p1, .1f);
            Gizmos.DrawSphere(seg.p2, .1f);

            Gizmos.color = Color.red;
            foreach (var split in seg.Subtract(cup, new List<LineSegment>())) {
                Gizmos.DrawLine(split.p1, split.p2);
            }
        }
    }
}
