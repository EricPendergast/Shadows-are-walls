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
    public Opaque caster;

    // TODO: Figure out what we need here
    [SerializeField]
    public Quad? currentShape;

    public void Init(Opaque caster) {
        this.caster = caster;

        for (int i = 0; i < 4; i++) {
            var edge = Util.CreateChild<CustomShadowEdge>(transform);
            edges.Add(edge);

            int iCaptured = i;
            edge.Init(() => {
                if (currentShape is Quad s) {
                    return s.GetSides()[iCaptured];
                } else {
                    return LineSegment.zero;
                }
            });
        }
    }

    public void SetShape(Quad? shape) {
        currentShape = shape;
    }

    void OnDestroy() {
        foreach (var edge in edges) {
            Destroy(edge.gameObject);
        }
    }
}
