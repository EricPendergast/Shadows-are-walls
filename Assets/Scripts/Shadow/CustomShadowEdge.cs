using UnityEngine;

public class CustomShadowEdge : ShadowEdge {
    System.Func<LineSegment> calculateTarget;

    public void Init(System.Func<LineSegment> calculateTarget) {
        this.calculateTarget = calculateTarget;
    }

    void Start() {
        SetTarget(calculateTarget());
    }

    protected override void FixedUpdate() {
        Debug.Log("CustomShadowEdge FixedUpdate");
        SetTarget(calculateTarget());
        base.FixedUpdate();
    }
}
