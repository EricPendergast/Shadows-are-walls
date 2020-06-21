using System.Collections.Generic;
using UnityEngine;

public class OpaqueSolidObject : LevelObject {
    protected override void Awake() {
        base.Awake();

        var opaque1 = gameObject.AddComponent<Opaque>();
        opaque1.crossSectionCallback = (Vector2) => {
            return new LineSegment(
                    transform.TransformPoint(new Vector2(-.5f, -.5f)),
                    transform.TransformPoint(new Vector2(.5f, .5f)));
        };
        opaque1.disableFrontFaceColliders = true;

        var opaque2 = gameObject.AddComponent<Opaque>();
        opaque2.crossSectionCallback = (Vector2) => {
            return new LineSegment(
                    transform.TransformPoint(new Vector2(.5f, -.5f)),
                    transform.TransformPoint(new Vector2(-.5f, .5f)));
        };
        opaque2.disableFrontFaceColliders = true;

        gameObject.layer = PhysicsHelper.opaqueLayer;
    }

    // This method is obsolete now, since it turns out having different cross
    // sections for different lights causes some subtle bugs.
    private LineSegment CrossSection(Vector2 cameraPos) {
        if (collider is BoxCollider2D boxColl) {
            List<Vector2> corners = new List<Vector2>{
                new Vector2(-.5f, -.5f),
                new Vector2(-.5f, .5f),
                new Vector2(.5f, .5f),
                new Vector2(.5f, -.5f)};
            for (int i = 0; i < 4; i++) {
                corners[i] = transform.TransformPoint(corners[i]);
            }

            Vector2 center = transform.TransformPoint(0,0,0);
            System.Func<Vector2, float> angleFromCenter = (Vector2 vec) => {
                return Vector2.SignedAngle((cameraPos - center), (cameraPos - vec));
            };

            Vector2 min = corners[0];
            Vector2 max = corners[0];
            for (int i = 1; i < 4; i++) {
                if (angleFromCenter(min) < angleFromCenter(corners[i])) {
                    min = corners[i];
                }
                if (angleFromCenter(max) > angleFromCenter(corners[i])) {
                    max = corners[i];
                }
            }

            return new LineSegment(min, max);
        }
        else {
            return LineSegment.zero;
        }
    }

    public override void DoSnapping() {
        if (snapToGrid) {
            SnapScale();
            SnapPosition();
        }
    }
}
