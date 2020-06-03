using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shadow : AllTracker<Shadow> {
    [SerializeField]
    private List<ShadowEdge> frontEdges = new List<ShadowEdge>();
    [SerializeField]
    private ShadowEdge rightEdge;
    [SerializeField]
    private ShadowEdge leftEdge;

    [SerializeField]
    private bool rightEdgeUpdated = false;
    [SerializeField]
    private bool leftEdgeUpdated = false;
    [SerializeField]
    private bool frontEdgesUpdated = false;

    [SerializeField]
    public Opaque caster;

    [SerializeField]
    public LightBase lightSource;

    // Says which side of each shadow edge is illuminated by 'lightSource'.
    // Notice that this isn't an array of fields; this value indicates the
    // illuminated side for all passed in line segments
    [SerializeField]
    DividesLight.Side illuminatedSide;

    public void Init(LightBase lightSource, DividesLight.Side illuminatedSide, Opaque caster) {
        this.caster = caster;
        this.lightSource = lightSource;
        this.illuminatedSide = illuminatedSide;
    }

    void OnDrawGizmosSelected() {
        var p1 = caster.CrossSection(lightSource.GetTargetPosition()).p1;
        var p2 = caster.CrossSection(lightSource.GetTargetPosition()).p2;
        //Gizmos.DrawLine(p1, p2);
        Gizmos.DrawSphere(p1, .1f);
        Gizmos.DrawSphere(p2, .1f);
        if (rightEdge != null) {
            Gizmos.color = Color.red;
            rightEdge.DrawGizmos();
        }
        if (leftEdge != null) {
            Gizmos.color = Color.blue;
            leftEdge.DrawGizmos();
        }
        Gizmos.color = Color.yellow;
        foreach (var frontEdge in frontEdges) {
            frontEdge.DrawGizmos();
        }
        Gizmos.color = Color.white;
    }

    public void SetRightEdge(LineSegment? right) {
        rightEdgeUpdated = true;
        SetTarget(ref rightEdge, right);
    }

    public void SetLeftEdge(LineSegment? left) {
        leftEdgeUpdated = true;
        SetTarget(ref leftEdge, left);
    }

    public void SetFrontEdges(IEnumerable<LineSegment> frontSegs) {
        frontEdgesUpdated = true;
        int count = 0;
        if (frontSegs != null) {
            foreach (var seg in frontSegs) {
                if (count >= frontEdges.Count) {
                    frontEdges.Add(null);
                }
                var edge = frontEdges[count];
                SetTarget(ref edge, seg);
                frontEdges[count] = edge;
                count++;
            }
        }

        for (int i = count; i < frontEdges.Count; i++) {
            DestroyShadowEdge(frontEdges[i]);
        }
        frontEdges.RemoveRange(count, frontEdges.Count - count);
    }

    private void SetTarget(ref ShadowEdge edge, LineSegment? target) {
        if (edge == null && target != null) {
            edge = Util.CreateChild<ShadowEdge>(transform);
            edge.Init(lightSource, illuminatedSide, caster);
        }
    
        if (edge != null && target == null) {
            DestroyShadowEdge(ref edge);
        }
    
        if (target is LineSegment t) {
            edge.SetTarget(t);
        }
    }

    public void DoFixedUpdate() {
        if (!rightEdgeUpdated) {
            SetRightEdge(null);
        }
        if (!leftEdgeUpdated) {
            SetLeftEdge(null);
        }
        if (!frontEdgesUpdated) {
            SetFrontEdges(null);
        }

        rightEdgeUpdated = false;
        leftEdgeUpdated = false;
        frontEdgesUpdated = false;
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        DestroyShadowEdge(ref rightEdge);
        DestroyShadowEdge(ref leftEdge);
        foreach (var frontEdge in frontEdges) {
            DestroyShadowEdge(frontEdge);
        }
    }

    private void DestroyShadowEdge(ref ShadowEdge shadowEdge) {
        DestroyShadowEdge(shadowEdge);
        shadowEdge = null;
    }
    private void DestroyShadowEdge(ShadowEdge shadowEdge) {
        if (shadowEdge != null) {
            var g = shadowEdge.gameObject;
            shadowEdge.Destroy();
            Destroy(g);
        }
    }

}
