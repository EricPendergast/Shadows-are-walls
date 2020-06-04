using UnityEngine;

public class ShadowLine : LevelObject {
    static Vector2[] points = new Vector2[]{new Vector2(-.5f, 0), new Vector2(.5f, 0)};

    protected override void Awake() {
        var myCollider = gameObject.AddComponent<BoxCollider2D>();

        myCollider.size = new Vector2(1, .01f);
        collider = myCollider;
        
        base.Awake();

        gameObject.layer = PhysicsHelper.opaqueLayer;
        body.constraints = RigidbodyConstraints2D.FreezeAll;
        collider.isTrigger = true;

        gameObject.AddComponent<Opaque>().crossSectionCallback = this.CrossSection;
    }

    private LineSegment CrossSection(Vector2 cameraPos) {
        return new LineSegment(
                transform.TransformPoint(points[0]),
                transform.TransformPoint(points[1]));
    }

    void OnDrawGizmos() {
        var crossSec = CrossSection(Vector2.zero);
        Gizmos.DrawLine(crossSec.p1, crossSec.p2);
    }
}
