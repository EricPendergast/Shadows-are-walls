using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

public partial class CameraRenderer {
    private void DrawGizmosBeforeFX() {
        #if UNITY_EDITOR
            if (Handles.ShouldRenderGizmos()) {
                context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
            }
        #endif
    }

    private void DrawGizmosAfterFX() {
        #if UNITY_EDITOR
            if (Handles.ShouldRenderGizmos()) {
                context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
            }
        #endif
    }

    private void PrepareForSceneWindow() {
        #if UNITY_EDITOR
            if (camera.cameraType == CameraType.SceneView) {
                ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
            }
        #endif
    }
}
