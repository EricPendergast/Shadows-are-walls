#if UNITY_EDITOR
using UnityEditor;

public partial class LampshadeRenderer {
    public void OnApertureAngleChange(float apertureAngle) {
        Undo.RecordObject(this, "Change aperture angle");
        this.apertureAngle = apertureAngle;
        BuildMesh();
    }
}

#endif
