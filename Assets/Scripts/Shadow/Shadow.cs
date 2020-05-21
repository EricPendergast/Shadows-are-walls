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
    [SerializeField]
    public Quad currentShape;

    public void Init(Opaque caster, LightBase light) {
        sourceLight = light;

        for (int i = 0; i < 4; i++) {
            var edge = Util.CreateChild<CustomShadowEdge>(transform);
            edges.Add(edge);

            int iCaptured = i;
            edge.Init(() => {
                LineSegment section = caster.CrossSection(sourceLight.transform.position);
                if (sourceLight.GetShadowShape(section) is Quad s) {
                    return s.GetSides()[iCaptured];
                } else {
                    return LineSegment.zero;
                }
            });
        }
    }

    void OnDestroy() {
        foreach (var edge in edges) {
            Destroy(edge.gameObject);
        }
    }
}
