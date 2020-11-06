#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public partial class LampshadeRenderer {
    public void OnApertureAngleChange(float apertureAngle) {
        EditorHelper.RecordObjectUndo(this, "Change aperture angle");
        this.apertureAngle = apertureAngle;
        EditorHelper.RecordObjectUndo(GetComponent<MeshFilter>(), "Change aperture angle");
        BuildMesh();
    }
}

#endif
