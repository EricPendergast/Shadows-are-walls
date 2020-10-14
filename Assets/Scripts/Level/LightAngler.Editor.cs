#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteAlways]
public partial class LightAngler : LevelObject {

    void UndoRecordAll() {
        foreach (Component c in gameObject.GetComponents<Component>()) {
            Undo.RecordObject(c, "Automatically set light aperture angle");
        }
        foreach (Component c in Util.AllChildrenComponentsIter(gameObject)) {
            Undo.RecordObject(c, "Automatically set light aperture angle");
        }
    }

    public float EditorGetCurrentAngle() {
        return currentAngle;
    }

    public void EditorSetCurrentAngle(float angle) {
        Undo.RecordObject(this, "Change current angle");
        currentAngle = angle;
        ApplySettings();
    }

    void Update() {
        if (!Application.isPlaying) {
            ApplySettings();
        }
    }

    private void OnDrawGizmos() {
        DrawGizmos(1);
    }

    protected override void OnDrawGizmosSelected() {
        DrawGizmos(20);
    }

    void DrawGizmos(float gizmoLength) {
        if (unconstrained) {
            return;
        }
        Gizmos.color = Color.red;
        Gizmos.DrawRay(body.position, Quaternion.Euler(0,0,body.rotation - angleConstraint)*Vector2.right*gizmoLength);
        Gizmos.DrawRay(body.position, Quaternion.Euler(0,0,body.rotation - (angleConstraint - apertureAngle))*Vector2.right*gizmoLength);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(body.position, Quaternion.Euler(0,0,body.rotation + angleConstraint)*Vector2.right*gizmoLength);
        Gizmos.DrawRay(body.position, Quaternion.Euler(0,0,body.rotation + (angleConstraint - apertureAngle))*Vector2.right*gizmoLength);
    }

    public void ApplySettings() {
        foreach (Component c in gameObject.GetComponents<Component>()) {
            Undo.RecordObject(c, "Automatically set light aperture angle");
        }
        DoSnapping();
        DetectConstraints();

        angleConstraint = Mathf.Max(angleConstraint, apertureAngle/2);
        currentAngle = ClampToConstraints(currentAngle);
        if (controled != null) {
            SetControledAngle(currentAngle);
            controled.SetTargetApertureAngle(apertureAngle);
            controled.transform.localPosition = Vector3.zero;
        }
    } 

    private float GetControledAngle() {
        return controled.GetComponent<Rigidbody2D>().rotation;
    }

    private void SetControledAngle(float angle) {
        controled.GetComponent<Rigidbody2D>().rotation = body.rotation + angle;
        // When this is called in the editor, the transform doesn't update
        // automatically, so we need to change it manually.
        controled.transform.localRotation = Quaternion.Euler(0,0,angle);
    }

    public override void DoSnapping() {
        if (!Application.isPlaying) {
            if (base.snapToGrid) {
                SnapPosition();
                body.position = transform.position;
            }
        }
    }

    private bool IsAngleUnoccupied(float angle) {
        foreach (RaycastHit2D hit in Physics2D.LinecastAll(body.position + Math.Rotate(Vector2.right, angle)*.1f, body.position + Math.Rotate(Vector2.right, angle)*.5f)) {
            if (PhysicsHelper.IsStatic(hit.rigidbody) && hit.rigidbody != body) {
                return false;
            }
        }

        return true;
    }

    // Uses raycasting to detect angular constraints on this light. For
    // example, if it is on a wall, this function will set body.rotation and
    // angleConstraint so that the light won't intersect the wall.
    private void DetectConstraints() {
        float inc = 45;

        var unoccupiedAngles = new List<bool>();

        for (int i = 0; i < 360/inc; i++) {
            unoccupiedAngles.Add(IsAngleUnoccupied((i+.5f)*inc));
        }
        Algorithm.Bounds bounds = Algorithm.FindContiguousWrapAround(unoccupiedAngles);

        if (bounds.IsEmpty() || bounds.IsFull()) {
            unconstrained = true;
            return;
        } else {
            unconstrained = false;
        }

        float lowerBound = (bounds.Lower()) * inc;
        float upperBound = (bounds.Upper()+1) * inc;
        if (upperBound < lowerBound) {
            lowerBound -= 360;
        }

        SetConstraints(lowerBound, upperBound);
    }

    void SetConstraints(float lowerBound, float upperBound) {
        if (Math.AnglesApproximatelyEqual(lowerBound, body.rotation - angleConstraint) &&
            Math.AnglesApproximatelyEqual(upperBound, body.rotation + angleConstraint)) {
            return;
        }

        float deltaRotation;
        {
            float newRotation = (upperBound + lowerBound)/2;
            deltaRotation = body.rotation - newRotation;
            body.rotation = newRotation;
        }

        transform.rotation = Quaternion.Euler(0,0,body.rotation);
        currentAngle += deltaRotation;
        angleConstraint = Math.CounterClockwiseAngleDifference(lowerBound, upperBound)/2;
    }
}

#endif
