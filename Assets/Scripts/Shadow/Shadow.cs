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
    private List<ShadowEdge> edges = new List<ShadowEdge>();
    [SerializeField]
    public LightBase sourceLight;

    public void Init(Opaque caster, LightBase light) {
        sourceLight = light;
        for (int i = 0; i < 4; i++) {
            var shadowEdgeGo = new GameObject();
            shadowEdgeGo.transform.parent = transform;
            var shadowEdge = shadowEdgeGo.AddComponent<CastedShadowEdge>();
            edges.Add(shadowEdge);
            shadowEdge.Init(caster, (CastedShadowEdge.EdgeType)i, light);
        }
    }

    void OnDestroy() {
        foreach (var edge in edges) {
            Destroy(edge);
        }
    }
}
