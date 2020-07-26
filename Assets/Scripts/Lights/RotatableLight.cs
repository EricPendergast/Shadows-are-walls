using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// General idea: Light finds all opaque objects in the scene, calculates all
// their shadows, tells all its shadow edges where they should be based on
// those calculations (setting their "targets"). It also handles rendering
//
// Notes about coordinates/terminology:
//      "right" and "left" are refered to as if you are facing in the
//      direction of the light
[RequireComponent(typeof(Rigidbody2D))]
public class RotatableLight : LightBase {
    [SerializeField]
    private bool DEBUG = false;

    private class DebugSnapshot {
        public Triangle actualViewTriangle;
        public Triangle targetViewTriangle;
    }
    private DebugSnapshot lastSnapshot;

    public float apertureAngle;
    public float distance;

    [SerializeField]
    public LightSettings settings;
    [SerializeField]
    public LightSettings plasticModeSettings;

    public float plasticMode = -1;
    public float plasticModeDuration = 1;

    private Mesh castLightMesh;


    private Dictionary<Opaque, Shadow> shadows = new Dictionary<Opaque, Shadow>();
    //public List<Opaque> DEBUG = new List<Opaque>();

    private LightEdge rightShadowEdge;
    private LightEdge leftShadowEdge;
    //private LightEdge farShadowEdge;

    // All visible shadows. This does not contain Shadow objects because
    // sometimes shadows get split into multiple line segments
    private List<LineSegment> trimmedShadows = new List<LineSegment>();

    private Triangle actualViewTriangle;
    private Triangle targetViewTriangle;

    [SerializeField]
    private GameObject shadowParent;

    private PolygonCollider2D visibleCollider;

    private Rigidbody2D _body;
    private Rigidbody2D body {
        get => _body == null ? (_body = GetComponent<Rigidbody2D>()) : _body;
    }

    private Rigidbody2D _edgeMountPoint;

    public override Rigidbody2D GetEdgeMountPoint() { if (_edgeMountPoint == null) {
            _edgeMountPoint = Util.CreateChild<Rigidbody2D>(transform);
            //_edgeMountPoint.constraints = RigidbodyConstraints2D.FreezeRotation;
            _edgeMountPoint.constraints = RigidbodyConstraints2D.FreezeAll;
            _edgeMountPoint.gravityScale = 0;
            _edgeMountPoint.gameObject.name = "Edge mount point";
        }
        return _edgeMountPoint;
    }

    public void SetTargetApertureAngle(float v) {
        apertureAngle = v;
    }

    public float GetTargetApertureAngle() {
        return apertureAngle;
    }


    public float GetTargetAngle() {
        return body.rotation;
    }

    public void SetTargetAngle(float v) {
        body.rotation = v;
    }

    public void SetTargetPosition(Vector2 p) {
        body.position = p;
    }

    public override Vector2 GetTargetPosition() {
        return body.position;
    }

    public LineSegment TargetFarEdge() {
        return new LineSegment(targetViewTriangle.p2, targetViewTriangle.p3);
    }

    public LineSegment TargetRightEdge() {
        return new LineSegment(targetViewTriangle.p1, targetViewTriangle.p3);
    }

    public LineSegment TargetLeftEdge() {
        return new LineSegment(targetViewTriangle.p1, targetViewTriangle.p2);
    }

    void CacheViewTriangles() {
        targetViewTriangle = CalculateTargetViewTriangle();
        actualViewTriangle = CalculateActualViewTriangle();
    }

    public Triangle CalculateTargetLocalViewTriangle() {
        Vector3 origin = Vector3.zero;
        Vector3 leftCorner = Quaternion.Euler(0f,0f, apertureAngle/2) * Vector2.right;
        Vector3 rightCorner = Quaternion.Euler(0f,0f, -apertureAngle/2) * Vector2.right;
        leftCorner *= distance;
        rightCorner *= distance;
        
        return new Triangle(origin, leftCorner, rightCorner);
    }

    public Triangle CalculateTargetViewTriangle() {
        // TODO SERIOUS: This shouldn't use transform, this will cause things to happen out of sync
        Triangle loc = CalculateTargetLocalViewTriangle();
        return new Triangle(
                transform.TransformPoint(loc.p1),
                transform.TransformPoint(loc.p2),
                transform.TransformPoint(loc.p3));
    }

    public Triangle CalculateActualViewTriangle() {
        var leftSeg = leftShadowEdge.GetActual().WithLength(distance);
        var rightSeg = rightShadowEdge.GetActual().WithLength(distance);
        if (leftSeg.p1 == leftSeg.p2 || rightSeg.p1 == rightSeg.p2) {
            return CalculateTargetViewTriangle();
        }
        return new Triangle((leftSeg.p1+rightSeg.p1)/2, leftSeg.p2, rightSeg.p2);
    }

    public Triangle CalculateActualLocalViewTriangle() {
        // TODO SERIOUS: This shouldn't use transform, this will cause things to happen out of sync
        Triangle loc = CalculateActualViewTriangle();
        return new Triangle(
                transform.InverseTransformPoint(loc.p1),
                transform.InverseTransformPoint(loc.p2),
                transform.InverseTransformPoint(loc.p3));
    }

    void OnDrawGizmos() {
        foreach (var side in CalculateTargetViewTriangle().GetSides()) {
            if (DEBUG) {
                Debug.Log(side.Angle());
            }
            Gizmos.DrawLine(side.p1, side.p2);
        }
        //DrawSnapshot(lastSnapshot);
    }

    void DrawSnapshot(DebugSnapshot snap) {
        if (snap == null) {
            return;
        }
        Gizmos.color = Color.red;
        foreach (var seg in snap.targetViewTriangle.GetSides()) {
            Gizmos.DrawLine(seg.p1, seg.p2);
        }
        Gizmos.color = Color.white;
        foreach (var seg in snap.actualViewTriangle.GetSides()) {
            Gizmos.DrawLine(seg.p1, seg.p2);
        }
    }

    //void OnDrawGizmosSelected() {
    //    Gizmos.color = Color.red;
    //    foreach (var seg in CalculateActualViewTriangle().GetSides()) {
    //        Gizmos.DrawLine(seg.p1, seg.p2);
    //    }
    //    Gizmos.color = Color.blue;
    //    foreach (var seg in CalculateTargetViewTriangle().GetSides()) {
    //        Gizmos.DrawLine(seg.p1, seg.p2);
    //    }
    //    Gizmos.color = Color.white;
    //}

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

    //void DrawLampshade() {
    //    Vector3 v1 = Vector3.zero;
    //    Vector3 v2 = Quaternion.Euler(0f,0f, angle/2) * Vector2.up;
    //    Vector3 v3 = Quaternion.Euler(0f,0f, -angle/2) * Vector2.up;
    //
    //    myMesh.vertices = new []{v1, v2, v3};
    //    myMesh.triangles = new []{0,1,2};
    //}

    void DrawCastedLight() {

        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        vertices.Add(GetTargetPosition());

        foreach (LineSegment shadow in trimmedShadows) {
            triangles.Add(vertices.Count);
            vertices.Add(shadow.p1);
            triangles.Add(vertices.Count);
            vertices.Add(shadow.p2);
            triangles.Add(0);
        }

        for (int i = 0; i < vertices.Count; i++) {
            vertices[i] = transform.InverseTransformPoint(vertices[i]);
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
        TakeSnapshot();

        UpdateKnownShadows();

        var shadowData = GetShadowData();

        trimmedShadows.Clear();
        foreach (var tuple in shadowData) {
            trimmedShadows.Add(tuple.Item1);
        }

        SendDataToShadows(shadowData);
        SendDataToLightEdges(shadowData);
    }

    void Update() {
        DrawCastedLight();
    }

    void DoForces() {
        //return;
        DoForces(rightShadowEdge);
        DoForces(leftShadowEdge);
    }

    void DoForces(LightEdge edge) {
        edge.MaxDifferenceFromTarget(out var point, out var difference);

        var currentSettings = plasticMode > 0 ? plasticModeSettings : settings;

        if (difference.magnitude > currentSettings.plasticModeDifferenceThreshold) {
            plasticMode = plasticModeDuration;
        }

        currentSettings = plasticMode > 0 ? plasticModeSettings : settings;
        edge.GetComponent<Rigidbody2D>().mass = currentSettings.mass;
        plasticMode -= Time.deltaTime;
    
        //if (difference.magnitude != 0) {
            //Debug.Log("difference magnitude " + difference.magnitude);
            //difference = difference.normalized * (difference.magnitude - .13f);
            // The angle change from actual to target

            var simpleAngleDelta = new LineSegment(body.position, point + difference).Angle(point);
            var angleDelta = simpleAngleDelta + body.angularVelocity*Time.deltaTime*currentSettings.velocityCorrectionConstant;
            //if (DEBUG) {
            //    Debug.Log(difference.magnitude);
            //}
            //Debug.Log("Simpleangledelta" + simpleAngleDelta);
            //Debug.Log(angleDelta);
            //angleDelta = Mathf.Clamp(angleDelta, -.5f, .5f);
            //Debug.Log("angle delta " + angleDelta);
            //var torque = PhysicsHelper.GetRotateToTorque(body, 0, angleDelta);
            //var torque = PhysicsHelper.GetRotateToTorque(body, 0, 0);
            //Debug.Log("test1 " + torque);
            //Debug.Log("test2 " + point);
            //Debug.Log("test3 " + difference);
            //Debug.Log("test5 " + angleDelta);
            //body.AddTorque(torque);
            //body.angularVelocity *= .9f;
            var torque = PhysicsHelper.GetSpringTorque(angleDelta, 0, 0, 0, currentSettings.correctionSpringConstant, currentSettings.correctionDampingConstant);
            body.AddTorque(torque);
            body.angularVelocity *= currentSettings.velocityMultiplier;
            //body.AddTorque(Mathf.Clamp(torque, -maxCorrectionTorque, maxCorrectionTorque));
            //body.angularVelocity *= Mathf.Lerp(1, .3f, difference.magnitude*5);
            //body.angularVelocity += edge.AngularVelocity();
            //body.angularVelocity /= 2;
            //if (difference.magnitude > .01) {
            //    body.angularVelocity *= .5f;
            //}
            //if (simpleAngleDelta * body.angularVelocity > 0) {
            //    StartCoroutine(SlowVelocityCoroutine());
            //    Debug.Log("Uh oh" + simpleAngleDelta);
            //}
            //var torque2 = PhysicsHelper.GetSpringTorque(body.rotation, targetAngle, body.angularVelocity, [>edge.AngularVelocity()<]0, toTargetSpringConstant, toTargetDampingConstant);
            //body.AddTorque(Mathf.Clamp(torque2, -maxToTargetTorque, maxToTargetTorque));
        //}
    }

    List<System.Tuple<LineSegment, Shadow>> GetShadowData() {
        var shadowCorrespondences = new List<System.Tuple<LineSegment, Shadow>>();

        foreach (Shadow s in shadows.Values) {
            if (s.caster.CrossSection(GetTargetPosition()) is LineSegment crossSec) {
                if (crossSec.Intersect(targetViewTriangle) is LineSegment seg) {
                    shadowCorrespondences.Add(System.Tuple.Create(seg, s));
                }
            }
        }
        shadowCorrespondences.Add(System.Tuple.Create(TargetFarEdge(), (Shadow)null));

        MinimalUnion<Shadow>.Calculate(ref shadowCorrespondences, GetTargetPosition(), Angle);

        shadowCorrespondences.Sort((s1, s2) => Angle(s1.Item1.Midpoint()).CompareTo(Angle(s2.Item1.Midpoint())));

        return shadowCorrespondences;
    }

    void SendDataToShadows(List<System.Tuple<LineSegment, Shadow>> shadowData) {
        var frontFacing = new Dictionary<Shadow, List<LineSegment>>();
        var leftFacing = new Dictionary<Shadow, LineSegment>();
        var rightFacing = new Dictionary<Shadow, LineSegment>();

        for (int i = 0; i < shadowData.Count; i++) {
            LineSegment? prevSeg = i == 0 ? (LineSegment?)null : shadowData[i-1].Item1;
            LineSegment? nextSeg = i == shadowData.Count-1 ? (LineSegment?)null : shadowData[i+1].Item1;
            LineSegment seg = shadowData[i].Item1;
            Shadow shadow = shadowData[i].Item2;

            if (shadow != null && seg.Length() > .0001) {
                if (!frontFacing.ContainsKey(shadow)) {
                    frontFacing.Add(shadow, new List<LineSegment>());
                }
                frontFacing[shadow].Add(seg);
                if (prevSeg is LineSegment prev) {
                    LineSegment right = new LineSegment(prev.p2, seg.p1);
                    if (right.Length() > .0001 && !right.GoesAwayFrom(GetTargetPosition())) {
                        rightFacing[shadow] = right;
                    }
                }
                if (nextSeg is LineSegment next) {
                    LineSegment left = new LineSegment(seg.p2, next.p1);
                    if (left.Length() > .0001 && left.GoesAwayFrom(GetTargetPosition())) {
                        leftFacing[shadow] = left;
                    }
                }
            }
        }

        foreach (var entry in frontFacing) {
            entry.Key.SetFrontEdges(entry.Value);
        }
        foreach (var entry in rightFacing) {
            entry.Key.SetRightEdge(entry.Value);
        }
        foreach (var entry in leftFacing) {
            entry.Key.SetLeftEdge(entry.Value);
        }
    }

    private void SetUpLightEdges() {
        rightShadowEdge = Util.CreateChild<LightEdge>(shadowParent.transform);
        rightShadowEdge.Init(this, ShadowEdgeBase.Side.left);
        leftShadowEdge = Util.CreateChild<LightEdge>(shadowParent.transform);
        leftShadowEdge.Init(this, ShadowEdgeBase.Side.right);
        //farShadowEdge = Util.CreateChild<LightEdge>(shadowParent.transform);
        //farShadowEdge.Init(this, DividesLight.Side.left);
    }

    private void SendDataToLightEdges(List<System.Tuple<LineSegment, Shadow>> shadowData) {

        var rightExtent = shadowData[0].Item1.p1;
        var leftExtent = shadowData[shadowData.Count-1].Item1.p2;
        rightShadowEdge.SetTarget(new LineSegment(GetTargetPosition(), rightExtent));
        leftShadowEdge.SetTarget(new LineSegment(GetTargetPosition(), leftExtent));
    }

    float Angle(Vector2 p) {
        return TargetRightEdge().Angle(p);
    }

    public override bool IsIlluminated(Vector2 point) {
        return !IsInDark(point);
    }

    public override bool IsInDark(Vector2 point) {
        if (!this.targetViewTriangle.Contains(point)) {
            return true;
        }

        foreach (LineSegment seg in trimmedShadows) {
            if (new Triangle(GetTargetPosition(), seg.p1, seg.p2).Contains(point)) {
                return false;
            }
        }
        return true;
    }

    void UpdateKnownShadows() {
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

    void TakeSnapshot() {
        if (lastSnapshot == null) {
            lastSnapshot = new DebugSnapshot();
        }

        lastSnapshot.actualViewTriangle = actualViewTriangle;
        lastSnapshot.targetViewTriangle = targetViewTriangle;
    }
}
