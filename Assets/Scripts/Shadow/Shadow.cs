using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// One of these will be created for each shadow (or equivalently, for each
// opaque object in view of a light).
//
// It will have a few kinematic edge colliders, which it uses to detect where
// it intersects other shadows
public class Shadow : MonoBehaviour {
    [SerializeField]
    private List<CustomShadowEdge> edges = new List<CustomShadowEdge>();
    [SerializeField]
    public LightBase sourceLight;
    public Opaque caster;

    public void Init(Opaque caster, LightBase light) {
        this.caster = caster;
        sourceLight = light;

        for (int i = 0; i < 4; i++) {
            var edge = Util.CreateChild<CustomShadowEdge>(transform);
            edges.Add(edge);

            int iCaptured = i;
            edge.Init(() => {
                if (GetCurrentShape() is Quad s) {
                    return s.GetSides()[iCaptured];
                } else {
                    return LineSegment.zero;
                }
            });
        }
    }

    public Quad? GetCurrentShape() {
        LineSegment section = caster.CrossSection(sourceLight.transform.position);
        return sourceLight.GetShadowShape(section);
    }

    void OnDestroy() {
        foreach (var edge in edges) {
            Destroy(edge.gameObject);
        }
    }
}
