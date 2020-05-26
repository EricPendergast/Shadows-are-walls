using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shadow : MonoBehaviour {
    [SerializeField]
    private List<ShadowEdge> frontEdges = new List<ShadowEdge>();
    [SerializeField]
    private ShadowEdge rightEdge;
    [SerializeField]
    private ShadowEdge leftEdge;

    [SerializeField]
    public Opaque caster;

    public LightBase lightSource;

    public void Init(Opaque caster, LightBase lightSource) {
        this.caster = caster;
        this.lightSource = lightSource;
    }

    public void SetRightEdge(LineSegment? right) {
        SetTarget(ref rightEdge, right);
    }

    public void SetLeftEdge(LineSegment? left) {
        SetTarget(ref leftEdge, left);
    }

    public void SetFrontEdges(IEnumerable<LineSegment> frontSegs) {
        int count = 0;
        foreach (var seg in frontSegs) {
            if (count >= frontEdges.Count) {
                frontEdges.Add(null);
            }
            var edge = frontEdges[count];
            SetTarget(ref edge, seg);
            frontEdges[count] = edge;
            count++;
        }

        for (int i = count; i < frontEdges.Count; i++) {
            DestroyShadowEdge(frontEdges[i]);
        }
        frontEdges.RemoveRange(count, frontEdges.Count - count);
    }

    private void SetTarget(ref ShadowEdge edge, LineSegment? target) {
        if (edge == null && target != null) {
            edge = Util.CreateChild<ShadowEdge>(transform);
        }
    
        if (edge != null && target == null) {
            DestroyShadowEdge(edge);
        }
    
        if (target is LineSegment t) {
            edge.SetTarget(t);
        }
    }

    void OnDestroy() {
        DestroyShadowEdge(rightEdge);
        DestroyShadowEdge(leftEdge);
        foreach (var frontEdge in frontEdges) {
            DestroyShadowEdge(frontEdge);
        }
    }

    private void DestroyShadowEdge(ShadowEdge shadowEdge) {
        if (shadowEdge != null) {
            var g = shadowEdge.gameObject;
            Destroy(shadowEdge);
            Destroy(g);
        }
    }

}
