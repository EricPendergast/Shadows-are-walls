using UnityEngine;

public class CustomShadowEdge : ShadowEdge {
    System.Func<LineSegment> calculateTarget;

    public void Init(System.Func<LineSegment> calculateTarget) {
        this.calculateTarget = calculateTarget;
    }

    protected override void FixedUpdate() {
        SetTarget(calculateTarget());
        base.FixedUpdate();
    }
}
