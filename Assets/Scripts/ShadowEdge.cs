using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class ShadowEdge : MonoBehaviour {
    private static Dictionary<int, ShadowEdge> allEdges = new Dictionary<int, ShadowEdge>();

    public enum EdgeType {
        front=0,
        p1=1,
        p2=2,
        back=3
    }
    [SerializeField]
    private EdgeType edgeType;
    [SerializeField]
    private Shadow shadow;
    [SerializeField]
    private Opaque caster;
    [SerializeField]
    private LightBase sourceLight;

    [SerializeField]
    private LineSegment target;
    bool initialized = false;

    public void Init(Opaque caster, EdgeType edgeType, LightBase light) {
        Assert.IsFalse(initialized);
        initialized = true;
        this.caster = caster;
        this.edgeType = edgeType;
        this.sourceLight = light;
        allEdges.Add(gameObject.GetInstanceID(), this);
        UpdateTarget();
    }

    void Start() {
        gameObject.layer = LayerMask.NameToLayer("ShadowEdge");
        Debug.Log(target.p1);
        transform.position = target.p1;
        transform.rotation = Quaternion.Euler(0, 0, target.Angle());
    }


    void OnDrawGizmos() {
        Gizmos.DrawLine(target.p1, target.p2);
        foreach (var i in GetIntersections()) {
            //Gizmos.DrawSphere(i, .1f);
        }
        //Gizmos.DrawSphere(target.p1, .1f);
    }

    void UpdateColliders() {
        var splits = GetIntersections();
        var colliders = new List<BoxCollider2D>(GetComponents<BoxCollider2D>());
        for (int i = colliders.Count; i < splits.Count + 1; i++) {
            colliders.Add(gameObject.AddComponent<BoxCollider2D>());
        }

        float totalLength = target.Length();
        System.Func<Vector2, float> dist = v => (target.p1 - v).magnitude;
        splits.Sort((v1, v2) => dist(v1).CompareTo(dist(v2)));

        splits.Add(target.p2);
        float prevDist = 0;

        for (int i = 0; i < colliders.Count; i++) {
            float currDist = dist(splits[i]);
            float width = currDist - prevDist;
            colliders[i].offset = new Vector2(prevDist+width/2, 0);
            colliders[i].size = new Vector2(width, .1f);
            prevDist = currDist;
        }
    }

    void UpdateTarget() {
        target = CalculateTarget();
    }

    LineSegment CalculateTarget() {
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

    void FixedUpdate() {
        UpdateTarget();
        UpdateColliders();
    }

    List<Vector2> GetIntersections() {
        var intersections = new List<Vector2>();
        foreach (ShadowEdge edge in allEdges.Values) {
            if (edge == this || edge.caster == this.caster) {
                continue;
            }
            if (target.Intersect(edge.target) is Vector2 intersec) {
                intersections.Add(intersec);
            }
        }
        return intersections;
    }

    void OnDestroy() {
        allEdges.Remove(gameObject.GetInstanceID());
    }
}
