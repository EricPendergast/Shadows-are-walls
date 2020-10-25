#if UNITY_EDITOR

using UnityEngine;

public partial class RotatableLight {
    public void EditorSetRotation(float angle) {
        EditorHelper.RecordObjectUndo(body, "Set angle");
        EditorHelper.RecordObjectUndo(transform, "Set angle");
        // When this is called in the editor, the transform doesn't update
        // automatically, so we need to change it manually.
        transform.rotation = Quaternion.Euler(0,0,angle);
        body.rotation = angle;
        EditorSetConstraints(rotationConstraints);
    }

    public float EditorGetRotation() {
        return body.rotation;
    }

    public float EditorGetTargetApertureAngle() {
        return apertureAngle;
    }

    public void EditorSetTargetApertureAngle(float angle) {
        EditorHelper.RecordObjectUndo(this, "Change aperture angle");
        apertureAngle = angle;
        apertureAngle = Mathf.Round(apertureAngle/5)*5;
        apertureAngle = Mathf.Clamp(apertureAngle, 1,
            Math.CounterClockwiseAngleDifference(
                rotationConstraints.lower,
                rotationConstraints.upper
            )
        );
        if (lampshadeRenderer != null) {
            lampshadeRenderer.OnApertureAngleChange(apertureAngle);
        }
        EditorSetConstraints(rotationConstraints);
    }

    public void EditorSetConstraints(RotationConstraints constraints) {
        EditorHelper.RecordObjectUndo(this, "Set constraints");
        EditorHelper.RecordObjectUndo(body, "Set constraints");
        EditorHelper.RecordObjectUndo(transform, "Set constraints");

        rotationConstraints = constraints;
        rotationConstraints.Apply(body, apertureAngle);
        transform.rotation = Quaternion.Euler(0, 0, body.rotation);
    }

    private void OnDrawGizmos() {
        foreach (var side in CalculateTargetViewTriangle().GetSides()) {
            var side1 = new Vector3(side.p1.x, side.p1.y, transform.position.z);
            var side2 = new Vector3(side.p2.x, side.p2.y, transform.position.z);
            Gizmos.DrawLine(side1, side2);
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

}

#endif
