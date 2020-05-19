using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// One of these will be created for each shadow (or equivalently, for each
// opaque object in view of a light).
//
// It will have a few kinematic edge colliders, which it uses to detect where
// it intersects other shadows
[ExecuteInEditMode]
public class Shadow : MonoBehaviour {
    [SerializeField]
    private Quad shape;

    [SerializeField]
    private List<EdgeCollider2D> edges = new List<EdgeCollider2D>();

    void OnDrawGizmos() {
        foreach (var seg in shape.GetSides()) {
            Gizmos.DrawLine((Vector2)transform.position + seg.p1, (Vector2)transform.position + seg.p2);
        }
    }

    void Start() {
        edges.Clear();
        edges.AddRange(gameObject.GetComponents<EdgeCollider2D>());

        for (int i = edges.Count; i < 4; i++) {
            edges.Add(gameObject.AddComponent<EdgeCollider2D>());
        }

    }

    void Update() {
        var sides = shape.GetSides();
        for (int i = 0 ; i < 4; i++) {
            var pt = edges[i].points;
            pt[0] = sides[i].p1;
            pt[1] = sides[i].p2;
            edges[i].points = pt;
        }
    }
}
