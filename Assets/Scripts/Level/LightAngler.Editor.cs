#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteAlways]
public partial class LightAngler : LevelObject {

    public float EditorGetCurrentAngle() {
        return controled.EditorGetRotation();
    }

    public void EditorSetCurrentAngle(float angle) {
        controled.EditorSetRotation(angle);
    }

    public float EditorGetApertureAngle() {
        return controled.EditorGetTargetApertureAngle();
    }

    public void EditorSetApertureAngle(float angle) {
        EditorHelper.RecordObjectUndo(this, "Set aperture angle");
        constraints.apertureAngle = angle;
        controled.EditorSetTargetApertureAngle(angle);
    }

    void Update() {
        if (!Application.isPlaying) {
            EditorHelper.RecordObjectUndo(this, "Undo Light angler");
            if (TryGetComponent<Snapper>(out var snapper)) {
                snapper.DoSnapping();
            }
            DetectConstraints();
            controled.EditorSetConstraints(constraints);
        }
    }

    private void OnDrawGizmos() {
        //DrawGizmos(1);
    }
    
    protected void OnDrawGizmosSelected() {
        //DrawGizmos(20);
    }
    
    void DrawGizmos(float gizmoLength) {
        if (constraints.unconstrained) {
            return;
        }
        //Gizmos.color = Color.red;
        //Gizmos.DrawRay(body.position, Quaternion.Euler(0,0,body.rotation - angleConstraint)*Vector2.right*gizmoLength);
        //Gizmos.DrawRay(body.position, Quaternion.Euler(0,0,body.rotation - (angleConstraint - apertureAngle))*Vector2.right*gizmoLength);
    //
        //Gizmos.color = Color.green;
        //Gizmos.DrawRay(body.position, Quaternion.Euler(0,0,body.rotation + angleConstraint)*Vector2.right*gizmoLength);
        //Gizmos.DrawRay(body.position, Quaternion.Euler(0,0,body.rotation + (angleConstraint - apertureAngle))*Vector2.right*gizmoLength);
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
        constraints.apertureAngle = controled.EditorGetTargetApertureAngle();

        float inc = 45;

        var unoccupiedAngles = new List<bool>();

        for (int i = 0; i < 360/inc; i++) {
            unoccupiedAngles.Add(IsAngleUnoccupied((i+.5f)*inc));
        }
        Algorithm.Bounds bounds = Algorithm.FindContiguousWrapAround(unoccupiedAngles);

        if (bounds.IsEmpty() || bounds.IsFull()) {
            constraints.unconstrained = true;
            return;
        } else {
            constraints.unconstrained = false;
        }

        float lowerBound = (bounds.Lower()) * inc;
        float upperBound = (bounds.Upper()+1) * inc;
        if (upperBound < lowerBound) {
            lowerBound -= 360;
        }

        constraints.lower = lowerBound;
        constraints.upper = upperBound;
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
