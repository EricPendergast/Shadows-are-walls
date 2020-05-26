using UnityEngine;
using UnityEngine.Assertions;

// This is definitely obsolete now
public class CastedShadowEdge : ShadowEdge {
    public enum EdgeType {
        front=0,
        p1=1,
        p2=2,
        back=3
    }
    [SerializeField]
    private Opaque caster;
    [SerializeField]
    private LightBase sourceLight;
    [SerializeField]
    private EdgeType edgeType;

    private bool initialized = false;

    public void Init(Opaque caster, EdgeType edgeType, LightBase light) {
        Assert.IsFalse(initialized);
        initialized = true;
        this.caster = caster;
        this.edgeType = edgeType;
        this.sourceLight = light;
    }

    protected LineSegment CalculateTarget() {
        return LineSegment.zero;
    }
}
