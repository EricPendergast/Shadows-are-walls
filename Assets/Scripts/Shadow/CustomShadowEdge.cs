using UnityEngine;

public class CustomShadowEdge : ShadowEdge {
    System.Func<LineSegment> calculateTarget;

    public void Init(System.Func<LineSegment> calculateTarget) {
        this.calculateTarget = calculateTarget;
    }

    protected override LineSegment CalculateTarget() {
        return calculateTarget();
    }
}
