#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

public partial class RotatableLight {
    public void SetAngle(float angle) {
        Undo.RecordObjects(new Object[]{body, transform}, "Set angle");
        body.rotation = body.rotation + angle;
        // When this is called in the editor, the transform doesn't update
        // automatically, so we need to change it manually.
        transform.localRotation = Quaternion.Euler(0,0,angle);
    }

    public void SetTargetApertureAngle(float angle) {
        Undo.RecordObject(this, "Change aperture angle");
        apertureAngle = angle;
        if (lampshadeRenderer != null) {
            lampshadeRenderer.OnApertureAngleChange(apertureAngle);
        }
    }

    private void OnDrawGizmos() {
        foreach (var side in CalculateTargetViewTriangle().GetSides()) {
            if (DEBUG) {
                Debug.Log(side.Angle());
            }
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
