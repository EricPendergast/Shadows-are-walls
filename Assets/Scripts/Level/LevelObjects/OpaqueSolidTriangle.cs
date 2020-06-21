using System.Collections.Generic;
using UnityEngine;

public class OpaqueSolidTriangle : SolidTriangle {
    protected override void Awake() {
        base.Awake();

        var opaque1 = gameObject.AddComponent<Opaque>();
        opaque1.crossSectionCallback = (Vector2) => {
            return new LineSegment(
                    transform.TransformPoint(p1),
                    transform.TransformPoint(p3));
        };
        opaque1.disableFrontFaceColliders = true;

        var opaque2 = gameObject.AddComponent<Opaque>();
        opaque2.crossSectionCallback = (Vector2) => {
            return new LineSegment(
                    transform.TransformPoint(p2),
                    transform.TransformPoint((p1 + p3)/2));
        };
        opaque2.disableFrontFaceColliders = true;

        gameObject.layer = PhysicsHelper.opaqueLayer;
    }
}
