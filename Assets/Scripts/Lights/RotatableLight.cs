//using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// General idea: Light finds all opaque objects in the scene, calculates all
// their shadows, tells all its shadow edges where they should be based on
// those calculations (setting their "targets"). It also handles rendering

public readonly struct LightViewTriangle {
    private readonly Triangle viewTriangle;

    public LightViewTriangle(Triangle viewTriangle) {
        this.viewTriangle = viewTriangle;
    }

    public Vector2 GetOrigin() {
        return viewTriangle.p1;
    }

    public LineSegment FarEdge() {
        return new LineSegment(viewTriangle.p2, viewTriangle.p3);
    }

    public LineSegment RightEdge() {
        return new LineSegment(viewTriangle.p1, viewTriangle.p3);
    }

    public LineSegment LeftEdge() {
        return new LineSegment(viewTriangle.p1, viewTriangle.p2);
    }

    public LineSegment? CalculateFrontFace(Opaque opaque) {
        if (opaque.CrossSection(GetOrigin()) is LineSegment crossSec) {
            return crossSec.Intersect(viewTriangle);
        }
        return null;
    }

    public float Angle(Vector2 point) {
        return RightEdge().Angle(point);
    }

    public int CompareAngles(LineSegment seg1, LineSegment seg2) {
        return Angle(seg1.Midpoint()).CompareTo(Angle(seg2.Midpoint()));
    }

    public bool Contains(Vector2 point) {
        return viewTriangle.Contains(point);
    }
}


// Notes about coordinates/terminology:
//      "right" and "left" are refered to as if you are facing in the
//      direction of the light
[RequireComponent(typeof(Rigidbody2D))]
public partial class RotatableLight : LightBase {
    [SerializeField]
    private bool DEBUG = false;

    [SerializeField]
    private float apertureAngle;
    [SerializeField]
    private float distance;

    [SerializeField]
    private LightSettings settings;
    [SerializeField]
    private LightSettings plasticModeSettings;

    [SerializeField]
    private float plasticMode = -1;
    [SerializeField]
    private float plasticModeDuration = 1;

    private Mesh castLightMesh;
    [SerializeField]
    private LampshadeRenderer lampshadeRenderer;

    [SerializeField]
    private ShadowCalculator shadowCalculator = new ShadowCalculator();

    private Dictionary<Opaque, Shadow> shadows = new Dictionary<Opaque, Shadow>();
    //public List<Opaque> DEBUG = new List<Opaque>();

    private LightEdge rightShadowEdge;
    private LightEdge leftShadowEdge;
    //private LightEdge farShadowEdge;

    //private Triangle actualViewTriangle;

    [SerializeField]
    private LightViewTriangle targetTriangle;

    [SerializeField]
    private GameObject shadowParent;

    private PolygonCollider2D visibleCollider;

    private Rigidbody2D _body;
    private Rigidbody2D body {
        get => _body == null ? (_body = GetComponent<Rigidbody2D>()) : _body;
    }

    private Rigidbody2D _edgeMountPoint;

    public struct RotationConstraints {
        public bool unconstrained;
        public float lower;
        public float upper;

        public void Apply(Rigidbody2D body, float apertureAngle) {
            if (!unconstrained) {
                var trueLower = lower + apertureAngle/2;
                if (body.rotation < trueLower) {
                    body.rotation = trueLower;
                    body.angularVelocity = Mathf.Max(body.angularVelocity, 0);
                }
                var trueUpper = upper - apertureAngle/2;
                if (body.rotation > trueUpper) {
                    body.rotation = trueUpper;
                    body.angularVelocity = Mathf.Min(body.angularVelocity, 0);
                }
            }
        }
    }

    private RotationConstraints rotationConstraints  = new RotationConstraints {
        unconstrained = true,
        lower = 0,
        upper = 0
    };

    public override Rigidbody2D GetEdgeMountPoint() {
        if (_edgeMountPoint == null) {
            _edgeMountPoint = Util.CreateChild<Rigidbody2D>(transform);
            //_edgeMountPoint.constraints = RigidbodyConstraints2D.FreezeRotation;
            _edgeMountPoint.constraints = RigidbodyConstraints2D.FreezeAll;
            _edgeMountPoint.gravityScale = 0;
            _edgeMountPoint.gameObject.name = "Edge mount point";
        }
        return _edgeMountPoint;
    }

    public void ApplyAngularAcceleration(float accel) {
        body.AddTorque(accel*body.inertia);
    }

    public void SetRotationConstraints(RotationConstraints constraints) {
        rotationConstraints = constraints;
    }

    public float GetRotation() {
        return body.rotation;
    }

    public override float GetAngularVelocity() {
        return body.angularVelocity;
    }

    public override Vector2 GetTargetPosition() {
        return body.position;
    }

    private void CacheViewTriangles() {
        // TODO: The order of the elements in the triangle are important, but
        // that is not made explicit anywhere. Find a way to fix that.
        targetTriangle = new LightViewTriangle(CalculateTargetViewTriangle());
        //actualViewTriangle = CalculateActualViewTriangle();
    }

    private Triangle CalculateTargetLocalViewTriangle() {
        Vector3 origin = Vector3.zero;
        Vector3 leftCorner = Quaternion.Euler(0f,0f, apertureAngle/2) * Vector2.right;
        Vector3 rightCorner = Quaternion.Euler(0f,0f, -apertureAngle/2) * Vector2.right;
        leftCorner *= distance;
        rightCorner *= distance;
        
        return new Triangle(origin, leftCorner, rightCorner);
    }

    private Triangle CalculateTargetViewTriangle() {
        // TODO SERIOUS: This shouldn't use transform, this will cause things to happen out of sync
        Triangle loc = CalculateTargetLocalViewTriangle();
        return new Triangle(
                transform.TransformPoint(loc.p1),
                transform.TransformPoint(loc.p2),
                transform.TransformPoint(loc.p3));
    }

    private Triangle CalculateActualViewTriangle() {
        var leftSeg = leftShadowEdge.GetActual().WithLength(distance);
        var rightSeg = rightShadowEdge.GetActual().WithLength(distance);
        if (leftSeg.p1 == leftSeg.p2 || rightSeg.p1 == rightSeg.p2) {
            return CalculateTargetViewTriangle();
        }
        return new Triangle((leftSeg.p1+rightSeg.p1)/2, leftSeg.p2, rightSeg.p2);
    }

    private Triangle CalculateActualLocalViewTriangle() {
        // TODO SERIOUS: This shouldn't use transform, this will cause things to happen out of sync
        Triangle loc = CalculateActualViewTriangle();
        return new Triangle(
                transform.InverseTransformPoint(loc.p1),
                transform.InverseTransformPoint(loc.p2),
                transform.InverseTransformPoint(loc.p3));
    }

    protected override void Awake() {
        base.Awake();

        shadowParent = gameObject;

        SetUpLightEdges();

        CacheViewTriangles();

        castLightMesh = Util.CreateMeshWithSharedMaterial(gameObject, Refs.instance.lightMaterial);

        visibleCollider = gameObject.AddComponent<PolygonCollider2D>();
        visibleCollider.isTrigger = true;

        body.inertia = 1;
    }

    private void DrawCastedLight() {
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        vertices.Add(GetTargetPosition());

        foreach (LineSegment shadow in shadowCalculator.GetShadowSegments()) {
            // The order of the calls to triangled.Add(...) is important
            // because of backface culling.

            if ((Vector2)vertices.Last() != shadow.p1) {
                vertices.Add(shadow.p1);
            }
            triangles.Add(vertices.Count - 1);

            triangles.Add(0);

            if ((Vector2)vertices.Last() != shadow.p2) {
                vertices.Add(shadow.p2);
            }
            triangles.Add(vertices.Count - 1);
        }

        for (int i = 0; i < vertices.Count; i++) {
            var vert = transform.InverseTransformPoint(vertices[i]);
            vert.z = transform.position.z;
            vertices[i] = vert;
        }

        if (castLightMesh.vertexCount < vertices.Count) {
            castLightMesh.vertices = vertices.ToArray();
            castLightMesh.triangles = triangles.ToArray();
        } else {
            castLightMesh.triangles = triangles.ToArray();
            castLightMesh.vertices = vertices.ToArray();
        }
    }

    public override void DoFixedUpdate() {
        DoForces();
        CacheViewTriangles();

        UpdateKnownShadows();

        shadowCalculator.CalculateShadows(shadows.Keys, targetTriangle);

        SendDataToShadows();
        SendDataToLightEdges();
    }

    private void Update() {
        DrawCastedLight();
    }

    private void DoForces() {
        DoForces(rightShadowEdge);
        DoForces(leftShadowEdge);
    }

    private void DoForces(LightEdge edge) {
        var edgeAccel = edge.GetAppliedAngularAcceleration();
        var accelTowardsCenter = edge.GetAppliedAccelTowardsCenter();
        if (edgeAccel != 0) {
            Debug.Log("Edge accel: " + edgeAccel);
            Debug.Log("Central Force" + accelTowardsCenter);
        }

        LightSettings s = settings;

        edge.SetInertia(s.inertia);

        var accel = -edge.GetAppliedAngularAcceleration()*s.mult;
        //accel = Mathf.Sign(accel) * Mathf.Sqrt(Mathf.Abs(accel));

        accelTowardsCenter*=s.centralAccelerationConstant;
        accel = Mathf.Sign(accel)*(Mathf.Abs(accel) + Mathf.Abs(accelTowardsCenter));

        accel = Mathf.Clamp(accel, -s.maxAccel, s.maxAccel);

        if (Mathf.Abs(accel) < s.minAccel) {
            accel = 0;
        }
        body.rotation -= Mathf.Clamp(edge.AngularDifferenceFromTarget()*s.resolveConstant, -s.maxResolve, s.maxResolve);

        rotationConstraints.Apply(body, apertureAngle);

        body.AddTorque(accel*body.inertia);
    }

    private void SetUpLightEdges() {
        rightShadowEdge = Util.CreateChild<LightEdge>(shadowParent.transform);
        rightShadowEdge.Init(this, ShadowEdgeBase.Side.left);
        leftShadowEdge = Util.CreateChild<LightEdge>(shadowParent.transform);
        leftShadowEdge.Init(this, ShadowEdgeBase.Side.right);
        //farShadowEdge = Util.CreateChild<LightEdge>(shadowParent.transform);
        //farShadowEdge.Init(this, DividesLight.Side.left);
    }

    private void SendDataToShadows() {
        foreach (var entry in shadowCalculator.GetFrontFacing()) {
            shadows[entry.Key].SetFrontEdges(entry.Value);
        }
        foreach (var entry in shadowCalculator.GetRightFacing()) {
            shadows[entry.Key].SetRightEdge(entry.Value);
        }
        foreach (var entry in shadowCalculator.GetLeftFacing()) {
            shadows[entry.Key].SetLeftEdge(entry.Value);
        }
    }

    private void SendDataToLightEdges() {
        rightShadowEdge.SetTarget(shadowCalculator.GetRightmostFace());
        leftShadowEdge.SetTarget(shadowCalculator.GetLeftmostFace());
    }

    public override bool IsIlluminated(Vector2 point) {
        return !IsInDark(point);
    }

    public override bool IsInDark(Vector2 point) {
        if (!targetTriangle.Contains(point)) {
            return true;
        }

        foreach (LineSegment seg in shadowCalculator.GetShadowSegments()) {
            if (new Triangle(GetTargetPosition(), seg.p1, seg.p2).Contains(point)) {
                return false;
            }
        }
        return true;
    }

    private void UpdateKnownShadows() {
        var opaqueSet = GetOpaqueObjectsInView();
    
        foreach (var opaque in opaqueSet) {
            if (!shadows.ContainsKey(opaque)) {
                Shadow s = Util.CreateChild<Shadow>(shadowParent.transform);
                s.Init(this, ShadowEdgeBase.Side.left, opaque);
                shadows.Add(opaque, s);
            }
        }
    
        HashSet<Opaque> toRemove = new HashSet<Opaque>();
        foreach (var opaque in shadows.Keys) {
            if (!opaqueSet.Contains(opaque)) {
                toRemove.Add(opaque);
            }
        }

        foreach (var opaque in toRemove) {
            Shadow s = shadows[opaque];
            s.Destroy();
            Destroy(s.gameObject);
            shadows.Remove(opaque);
        }
    }

    private HashSet<Opaque> GetOpaqueObjectsInView() {
        visibleCollider.SetPath(0, CalculateActualLocalViewTriangle().AsList());
        var overlap = new List<Collider2D>();
        visibleCollider.OverlapCollider(new ContactFilter2D{useTriggers=true, layerMask = PhysicsHelper.opaqueLayer}, overlap);

        visibleCollider.SetPath(0, CalculateTargetLocalViewTriangle().AsList());
        visibleCollider.OverlapCollider(new ContactFilter2D{useTriggers=true, layerMask = PhysicsHelper.opaqueLayer}, overlap);
        
        
        var opaqueSet = new HashSet<Opaque>();
        foreach (var hit in overlap) {
            foreach (var opaque in hit.GetComponents<Opaque>()) {
                opaqueSet.Add(opaque);
            }
        }

        return opaqueSet;
    }
}


class ShadowCalculator {
    private List<System.Tuple<LineSegment, Opaque>> sortedFrontFaces = new List<System.Tuple<LineSegment, Opaque>>();
    //private List<LineSegment> sortedFrontFaces = new List<LineSegment>();
    private Dictionary<Opaque, List<LineSegment>> frontFacing = new Dictionary<Opaque, List<LineSegment>>();
    private Dictionary<Opaque, LineSegment> leftFacing = new Dictionary<Opaque, LineSegment>();
    private Dictionary<Opaque, LineSegment> rightFacing = new Dictionary<Opaque, LineSegment>();

    private LineSegment rightmostFace;
    private LineSegment leftmostFace;

    public void CalculateShadows(IEnumerable<Opaque> opaqueObjs, LightViewTriangle lightTriangle) {
        CalculateSortedFrontFaces(opaqueObjs, lightTriangle);
        CalculateShadowFaces(lightTriangle);
    }

    public IEnumerable<LineSegment> GetShadowSegments() {
        foreach (var tup in sortedFrontFaces) {
            yield return tup.Item1;
        }
    }

    public Dictionary<Opaque, List<LineSegment>> GetFrontFacing() {
        return frontFacing;
    }

    public Dictionary<Opaque, LineSegment> GetLeftFacing() {
        return leftFacing;
    }

    public Dictionary<Opaque, LineSegment> GetRightFacing() {
        return rightFacing;
    }

    public LineSegment GetRightmostFace() {
        return rightmostFace;
    }

    public LineSegment GetLeftmostFace() {
        return leftmostFace;
    }

    private void CalculateSortedFrontFaces(
            IEnumerable<Opaque> opaqueObjs, LightViewTriangle lightTriangle) {

        sortedFrontFaces.Clear();

        foreach (Opaque opaque in opaqueObjs) {
            if (lightTriangle.CalculateFrontFace(opaque) is LineSegment frontFace) {
                sortedFrontFaces.Add(System.Tuple.Create(frontFace, opaque));
            }
        }
        sortedFrontFaces.Add(System.Tuple.Create(lightTriangle.FarEdge(), (Opaque)null));

        MinimalUnion<Opaque>.CalculateAndSort(ref sortedFrontFaces, lightTriangle.GetOrigin(), lightTriangle.Angle);
    }

    private void CalculateShadowFaces(LightViewTriangle lightTriangle) {
        frontFacing.Clear();
        leftFacing.Clear();
        rightFacing.Clear();
        for (int i = 0; i < sortedFrontFaces.Count; i++) {
            LineSegment? prevSeg = i == 0 ? (LineSegment?)null : sortedFrontFaces[i-1].Item1;
            LineSegment? nextSeg = i == sortedFrontFaces.Count-1 ? (LineSegment?)null : sortedFrontFaces[i+1].Item1;
            LineSegment seg = sortedFrontFaces[i].Item1;
            Opaque opaque = sortedFrontFaces[i].Item2;

            if (opaque != null) {
                if (seg.Length() > .0001) {
                    if (!frontFacing.ContainsKey(opaque)) {
                        frontFacing.Add(opaque, new List<LineSegment>());
                    }
                    frontFacing[opaque].Add(seg);
                }
                if (prevSeg is LineSegment prev) {
                    LineSegment right = new LineSegment(prev.p2, seg.p1);
                    if (right.Length() > .0001 && !right.GoesAwayFrom(lightTriangle.GetOrigin())) {
                        rightFacing[opaque] = right;
                    }
                }
                if (nextSeg is LineSegment next) {
                    LineSegment left = new LineSegment(seg.p2, next.p1);
                    if (left.Length() > .0001 && left.GoesAwayFrom(lightTriangle.GetOrigin())) {
                        leftFacing[opaque] = left;
                    }
                }
            }
        }

        var rightExtent = sortedFrontFaces[0].Item1.p1;
        var leftExtent = sortedFrontFaces[sortedFrontFaces.Count-1].Item1.p2;

        rightmostFace = new LineSegment(lightTriangle.GetOrigin(), rightExtent);
        leftmostFace = new LineSegment(lightTriangle.GetOrigin(), leftExtent);
    }
}
