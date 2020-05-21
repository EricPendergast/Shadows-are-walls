using System.Collections.Generic;
using UnityEngine;

public class Polygon {
    public static void DrawQuad(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, List<Vector3> verts, List<int> indicies) {
        int start = verts.Count;
        verts.AddRange(new Vector3[]{p1, p2, p3, p4});
        indicies.AddRange(new int[]{
            start+0, start+1, start+2,
            start+0, start+2, start+3});
    }
}
