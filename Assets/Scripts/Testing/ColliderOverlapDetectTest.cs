using UnityEngine;
using System.Collections.Generic;

public class ColliderOverlapDetectTest : MonoBehaviour {
    public Vector2 targetPoint1;
    public Vector2 targetPoint2;
    public Vector2 targetPoint3;
    private PolygonCollider2D visibleCollider;
    private Rigidbody2D body;
    public List<Opaque> overlaps = new List<Opaque>();



    void OnDrawGizmos() {
        Triangle tri = new Triangle(targetPoint1, targetPoint2, targetPoint3);
        foreach (var seg in tri.GetSides()) {
            Gizmos.DrawLine(
                    transform.TransformPoint(seg.p1),
                    transform.TransformPoint(seg.p2));
        }

    }

    protected void Awake() {
        body = GetComponent<Rigidbody2D>();

        visibleCollider = gameObject.AddComponent<PolygonCollider2D>();
        visibleCollider.isTrigger = true;
    }

    public void FixedUpdate() {
        transform.rotation = Quaternion.Euler(0,0,body.rotation);
        transform.position = body.position;

        UpdateKnownShadows();
    }

    void UpdateKnownShadows() {
        // TODO: This should be larger than the actual view triangle. This may
        // be why there are problems with update loops. Maybe its fine if we
        // can get the intersections without a physics iteration.
        visibleCollider.SetPath(0, new Vector2[] {targetPoint1, targetPoint2, targetPoint3});
        var overlap = new List<Collider2D>();
        visibleCollider.OverlapCollider(new ContactFilter2D{useTriggers=true, layerMask = PhysicsHelper.opaqueLayer}, overlap);

        overlaps.Clear();
        foreach (var coll in overlap) {
            foreach (var opaque in coll.GetComponents<Opaque>()) {
                this.overlaps.Add(opaque);
            }
        }
    }
}
