using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SolidTriangle), true)]
[CanEditMultipleObjects]
public class SolidTriangleEditor : Editor {
    public override void OnInspectorGUI() {
        base.DrawDefaultInspector();
    }

    public void OnSceneGUI() {
        if (!Application.isPlaying) {
            var tri = target as SolidTriangle;

            if (tri.editing) {
                DoPositionHandle(ref tri.p1);
                DoPositionHandle(ref tri.p2);
                DoPositionHandle(ref tri.p3);
            }

            tri.ApplySettings();
        }
    }

    void DoPositionHandle(ref Vector2 localPoint) {
        var obj = target as SolidTriangle;

        var globalPoint = obj.transform.TransformPoint(localPoint);

        EditorGUI.BeginChangeCheck();
        var newGlobalPoint = Handles.PositionHandle(globalPoint, Quaternion.identity);
        if (EditorGUI.EndChangeCheck()) {
             Undo.RecordObject(obj, "Change movement path");
             localPoint = obj.transform.InverseTransformPoint(newGlobalPoint);
        }
    }
}
