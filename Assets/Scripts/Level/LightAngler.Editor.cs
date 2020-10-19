#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteAlways]
public partial class LightAngler : LevelObject {

    private void OnAboutToChange(Object obj, string msg) {
        Undo.RecordObject(obj, msg);
        PrefabUtility.RecordPrefabInstancePropertyModifications(obj);
    }

    public float EditorGetCurrentAngle() {
        return currentAngle;
    }

    public void EditorSetCurrentAngle(float angle) {
        OnAboutToChange(this, "Change current angle");
        currentAngle = angle;
        ApplySettings();
    }

    public float EditorGetApertureAngle() {
        return apertureAngle;
    }

    public void EditorSetApertureAngle(float angle) {
        OnAboutToChange(this, "Change aperture angle");
        apertureAngle = angle;
        ApplySettings();
    }

    void Update() {
        if (!Application.isPlaying) {
            ApplySettings();
        }
    }

    private void OnDrawGizmos() {
        //DrawGizmos(1);
    }
    
    protected void OnDrawGizmosSelected() {
        //DrawGizmos(20);
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
        OnAboutToChange(this, "Undo Light angler");
        if (TryGetComponent<Snapper>(out var snapper)) {
            snapper.DoSnapping();
        }
        DetectConstraints();

        apertureAngle = Mathf.Round(apertureAngle/5)*5;
        apertureAngle = Mathf.Clamp(apertureAngle, 1, angleConstraint*2);

        currentAngle = ClampToConstraints(currentAngle);
        if (controled != null) {
            controled.SetAngle(currentAngle);
            controled.SetTargetApertureAngle(apertureAngle);
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


    // Reverts transform.position.z back to the prefab. This code was used once
    // to revert the z coordinate of all the light anglers, because for some
    // reason they were all modified (and also the wrong value). I will keep it
    // around in case I need it again.
    void RevertLocalPositionZ() {
        var oldPosition = transform.localPosition;
        PrefabUtility.RevertPropertyOverride(new SerializedObject(transform).FindProperty("m_LocalPosition"), InteractionMode.UserAction);
        oldPosition.z = transform.localPosition.z;
        transform.localPosition = oldPosition;
    }
}

#endif
