//using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
public class RotatableLight : LightBase {
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

    //[SerializeField]
    //private ShadowCalculator shadowCalculator;

    private Dictionary<Opaque, Shadow> shadows = new Dictionary<Opaque, Shadow>();
    //public List<Opaque> DEBUG = new List<Opaque>();

    private LightEdge rightShadowEdge;
    private LightEdge leftShadowEdge;
    //private LightEdge farShadowEdge;

    // All visible shadows. This does not contain Shadow objects because
    // sometimes shadows get split into multiple line segments
    private List<LineSegment> trimmedShadows = new List<LineSegment>();

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
        if (lampshadeRenderer) {
            lampshadeRenderer.OnApertureAngleChange(apertureAngle);
        }
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

    private void OnDrawGizmos() {
        foreach (var side in CalculateTargetViewTriangle().GetSides()) {
            if (DEBUG) {
                Debug.Log(side.Angle());
            }
            Gizmos.DrawLine(side.p1, side.p2);
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

    private void DrawCastedLight() {

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
        //TakeSnapshot();

        UpdateKnownShadows();

        var shadowFrontFaces = CalculateShadowFrontFaces();

        trimmedShadows.Clear();
        foreach (var tuple in shadowFrontFaces) {
            trimmedShadows.Add(tuple.Item1);
        }

        SendDataToShadows(shadowFrontFaces);
        SendDataToLightEdges(shadowFrontFaces);
    }

    private void Update() {
        DrawCastedLight();
    }

    private void DoForces() {
        //return;
        DoForces(rightShadowEdge);
        DoForces(leftShadowEdge);
    }

    private void DoForces(LightEdge edge) {
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

    private List<System.Tuple<LineSegment, Shadow>> CalculateShadowFrontFaces() {
        var shadowFrontFaces = new List<System.Tuple<LineSegment, Shadow>>();

        foreach (Shadow s in shadows.Values) {
            if (targetTriangle.CalculateFrontFace(s.caster) is LineSegment frontFace) {
                shadowFrontFaces.Add(System.Tuple.Create(frontFace, s));
            }
        }
        shadowFrontFaces.Add(System.Tuple.Create(targetTriangle.FarEdge(), (Shadow)null));

        MinimalUnion<Shadow>.Calculate(ref shadowFrontFaces, GetTargetPosition(), targetTriangle.Angle);

        shadowFrontFaces.Sort((s1, s2) => targetTriangle.CompareAngles(s1.Item1, s2.Item1));

        return shadowFrontFaces;
    }

    private void SendDataToShadows(List<System.Tuple<LineSegment, Shadow>> shadowData) {
        var frontFacing = new Dictionary<Shadow, List<LineSegment>>();
        var leftFacing = new Dictionary<Shadow, LineSegment>();
        var rightFacing = new Dictionary<Shadow, LineSegment>();

        for (int i = 0; i < shadowData.Count; i++) {
            LineSegment? prevSeg = i == 0 ? (LineSegment?)null : shadowData[i-1].Item1;
            LineSegment? nextSeg = i == shadowData.Count-1 ? (LineSegment?)null : shadowData[i+1].Item1;
            LineSegment seg = shadowData[i].Item1;
            Shadow shadow = shadowData[i].Item2;

            if (shadow != null) {
                if (seg.Length() > .0001) {
                    if (!frontFacing.ContainsKey(shadow)) {
                        frontFacing.Add(shadow, new List<LineSegment>());
                    }
                    frontFacing[shadow].Add(seg);
                }
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

        // TODO: This is problematic because it is not obvious that shadowData
        // has to be sorted this way
        var rightExtent = shadowData[0].Item1.p1;
        var leftExtent = shadowData[shadowData.Count-1].Item1.p2;
        rightShadowEdge.SetTarget(new LineSegment(GetTargetPosition(), rightExtent));
        leftShadowEdge.SetTarget(new LineSegment(GetTargetPosition(), leftExtent));
    }

    public override bool IsIlluminated(Vector2 point) {
        return !IsInDark(point);
    }

    public override bool IsInDark(Vector2 point) {
        if (!targetTriangle.Contains(point)) {
            return true;
        }

        foreach (LineSegment seg in trimmedShadows) {
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


// TODO: Partially completed refactor
//class ShadowCalculator {
//    private List<LineSegment> trimmedShadows = new List<LineSegment>();
//    private Dictionary<Opaque, List<LineSegment>> frontFacing = new Dictionary<Opaque, List<LineSegment>>();
//    private Dictionary<Opaque, LineSegment> leftFacing = new Dictionary<Opaque, LineSegment>();
//    private Dictionary<Opaque, LineSegment> rightFacing = new Dictionary<Opaque, LineSegment>();
//
//    public void CalculateShadows(IEnumerable<Opaque> opaqueObjs) {
//        var shadowFrontFaces = CalculateShadowFrontFaces(opaqueObjs);
//        GenerateLeftRightFront(shadowFrontFaces);
//
//        trimmedShadows.Clear();
//        foreach (var tuple in shadowFrontFaces) {
//            trimmedShadows.Add(tuple.Item1);
//        }
//    }
//
//    private void GenerateLeftRightFront(List<System.Tuple<LineSegment, Opaque>> frontFaces) {
//        frontFacing.Clear();
//        leftFacing.Clear();
//        rightFacing.Clear();
//        //var frontFacing = new Dictionary<Opaque, List<LineSegment>>();
//        //var leftFacing = new Dictionary<Opaque, LineSegment>();
//        //var rightFacing = new Dictionary<Opaque, LineSegment>();
//
//        for (int i = 0; i < frontFaces.Count; i++) {
//            LineSegment? prevSeg = i == 0 ? (LineSegment?)null : frontFaces[i-1].Item1;
//            LineSegment? nextSeg = i == frontFaces.Count-1 ? (LineSegment?)null : frontFaces[i+1].Item1;
//            LineSegment seg = frontFaces[i].Item1;
//            Opaque caster = frontFaces[i].Item2;
//
//            if (caster != null) {
//                if (seg.Length() > .0001) {
//                    if (!frontFacing.ContainsKey(caster)) {
//                        frontFacing.Add(caster, new List<LineSegment>());
//                    }
//                    frontFacing[caster].Add(seg);
//                }
//                if (prevSeg is LineSegment prev) {
//                    LineSegment right = new LineSegment(prev.p2, seg.p1);
//                    if (right.Length() > .0001 && !right.GoesAwayFrom(GetTargetPosition())) {
//                        rightFacing[caster] = right;
//                    }
//                }
//                if (nextSeg is LineSegment next) {
//                    LineSegment left = new LineSegment(seg.p2, next.p1);
//                    if (left.Length() > .0001 && left.GoesAwayFrom(GetTargetPosition())) {
//                        leftFacing[caster] = left;
//                    }
//                }
//            }
//        }
//    }
//
//    private float Angle(Vector2 p) {
//        return TargetRightEdge().Angle(p);
//    }
//
//    private List<System.Tuple<LineSegment, Opaque>> CalculateShadowFrontFaces(IEnumerable<Opaque> opaqueObjs) {
//        var frontFaces = new List<System.Tuple<LineSegment, Opaque>>();
//
//        foreach (Opaque opaque in opaqueObjs) {
//            if (opaque.CrossSection(GetTargetPosition()) is LineSegment crossSec) {
//                if (crossSec.Intersect(targetViewTriangle) is LineSegment seg) {
//                    frontFaces.Add(System.Tuple.Create(seg, opaque));
//                }
//            }
//        }
//        frontFaces.Add(System.Tuple.Create(TargetFarEdge(), (Shadow)null));
//
//        MinimalUnion<Shadow>.Calculate(ref frontFaces, GetTargetPosition(), Angle);
//
//        frontFaces.Sort((s1, s2) => Angle(s1.Item1.Midpoint()).CompareTo(Angle(s2.Item1.Midpoint())));
//
//        return frontFaces;
//    }
//
//    public List<LineSegment> GetShadowSegments() {
//        return trimmedShadows;
//    }
//}
