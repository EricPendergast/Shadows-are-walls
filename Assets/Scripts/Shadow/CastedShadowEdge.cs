using UnityEngine;
using UnityEngine.Assertions;

// This is probably obsolete now
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

    protected override LineSegment CalculateTarget() {
        LineSegment sec = caster.CrossSection(sourceLight.transform.position);
        if (sourceLight.GetShadowShape(sec) is Quad s) {
            switch (edgeType) {
                case EdgeType.front:
                    return new LineSegment(s.p1, s.p4);
                case EdgeType.back:
                    return new LineSegment(s.p2, s.p3);;
                case EdgeType.p1:
                    return new LineSegment(s.p1, s.p2);;
                case EdgeType.p2:
                    return new LineSegment(s.p4, s.p3);;
                default:
                    return LineSegment.zero;
            }
        } else {
            return LineSegment.zero;
        }
    }
}
