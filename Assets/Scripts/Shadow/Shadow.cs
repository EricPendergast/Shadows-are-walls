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

    // This method takes a list of edges in order by angle, and deduces whether
    // that list includes the right and/or left facing edges.
    public void SetEdges(List<LineSegment> newEdges) {
        int frontFacingStart = 0;
        int frontFacingEnd = newEdges.Count;

        if (newEdges[0].IsInLineWith(lightSource.transform.position)) {
            SetTarget(ref leftEdge, newEdges[0]);
            frontFacingStart++;
        } else {
            SetTarget(ref leftEdge, null);
        }

        if (newEdges[newEdges.Count-1].IsInLineWith(lightSource.transform.position)) {
            SetTarget(ref rightEdge, newEdges[newEdges.Count-1]);
            frontFacingEnd--;
        } else {
            SetTarget(ref rightEdge, null);
        }
        
        IEnumerable<LineSegment> getFrontSegments() {
            for (int i = frontFacingStart; i < frontFacingEnd; i++) {
                yield return newEdges[i];
            }
        }

        SetFrontEdges(getFrontSegments());
    }

    private void SetFrontEdges(IEnumerable<LineSegment> frontSegs) {
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
